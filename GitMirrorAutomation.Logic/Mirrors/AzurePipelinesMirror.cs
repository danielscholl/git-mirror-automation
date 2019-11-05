using GitMirrorAutomation.Logic.Config;
using GitMirrorAutomation.Logic.Scanners;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Mirrors
{
    public class AzurePipelinesMirror : IMirrorService
    {
        private static readonly Regex _devOpsRegex = new Regex(@"https:\/\/dev\.azure\.com\/([^/?&# ]+)/([^/?&#]+)");

        private readonly HttpClient _httpClient;
        private readonly MirrorConfig _config;
        private readonly string _devOpsAccount;
        private readonly string _devOpsProject;
        private int? _buildToCloneId;
        private readonly IRepositorySource _repositorySource;
        private readonly ILogger _log;

        public AzurePipelinesMirror(
            MirrorConfig config,
            IRepositorySource repositoryScanner,
            ILogger log)
        {
            _config = config;
            _repositorySource = repositoryScanner;

            var match = _devOpsRegex.Match(config.Target);
            if (!match.Success)
                throw new ArgumentException("Expected a valid devops account url but got: " + config.Target);

            _devOpsAccount = match.Groups[1].Value;
            _devOpsProject = match.Groups[2].Value;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri($"https://dev.azure.com/{_devOpsAccount}/{_devOpsProject}/_apis/")
            };
            var token = ""; // TODO: load from keyvault
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", token))));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _log = log;
        }

        public async Task<Mirror[]> GetExistingMirrorsAsync(CancellationToken cancellationToken)
        {
            var mirrors = new List<Mirror>();
            var builds = await GetBuildsAsync(cancellationToken);
            var mirrorBuilds = builds.Where(b => b.Name.StartsWith(_config.BuildNamePrefix)).ToArray();
            _log.LogInformation($"Looking for existing mirrors in {builds.Length} builds ({mirrorBuilds.Length} of those are mirror builds)..");
            foreach (var build in mirrorBuilds)
            {
                if (!_buildToCloneId.HasValue &&
                    _config.BuildToClone.Equals(build.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _buildToCloneId = build.Id;
                }

                var repoName = build.Name.Substring(_config.BuildNamePrefix.Length);
                var expectedRepoUrl = _repositorySource.GetUrlForRepository(repoName);

                // repo isn't loaded in overall list, so get definition
                // to ensure build is using correct source repo
                var buildDefinition = await GetBuildDefinitionAsync(build.Id, cancellationToken);
                var buildWithRepo = JsonSerializer.Deserialize<Build>(buildDefinition.GetRawText(), JsonSettings.Default);
                var foundUrl = buildWithRepo.Repository.Url;
                if (foundUrl != expectedRepoUrl)
                    throw new NotSupportedException($"Expected build '{build.Name}' to use repository '{expectedRepoUrl}' but it uses '{buildWithRepo.Repository.Url}'");

                mirrors.Add(new Mirror
                {
                    BuildName = build.Name,
                    Id = build.Id,
                    Repository = repoName
                });
            }
            if (!_buildToCloneId.HasValue)
                throw new InvalidOperationException($"Could not find the build to clone '{_config.BuildToClone}' while scanning builds in Azure DevOps");

            return mirrors.ToArray();
        }

        public async Task SetupMirrorAsync(string repository, CancellationToken cancellationToken)
        {
            if (!_buildToCloneId.HasValue)
                throw new InvalidOperationException($"Must call {nameof(GetExistingMirrorsAsync)} first before setting up a new mirror!");

            var buildDefinition = await GetBuildDefinitionAsync(_buildToCloneId.Value, cancellationToken);
            // real inefficient but there seems to be no way to modify a JsonElement + this usually isn't executed a million times..
            var jObject = JObject.Parse(buildDefinition.GetRawText());
            jObject["repository"]["url"] = _repositorySource.GetUrlForRepository(repository);

            if (_repositorySource.Type != "Github")
                throw new NotSupportedException($"Currently only github is a supported repository source!");

            // id is needed and seems to be dependent on type of source
            jObject["repository"]["id"] = "Github/" + repository;
            jObject["repository"]["name"] = "Github/" + repository;
            jObject["name"] = _config.BuildNamePrefix + repository;

            var json = jObject.ToString();
            var response = await _httpClient.PostAsync("build/definitions?api-version=5.1", new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        private async Task<JsonElement> GetBuildDefinitionAsync(int id, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"build/definitions/{id}?api-version=5.1");
            response.EnsureSuccessStatusCode();
            return await JsonSerializer.DeserializeAsync<JsonElement>(await response.Content.ReadAsStreamAsync(), JsonSettings.Default, cancellationToken);
        }

        private Task<Build[]> GetBuildsAsync(CancellationToken cancellationToken)
            => GetCollectionAsync<Build>("build/definitions?api-version=5.1", cancellationToken);

        private async Task<T[]> GetCollectionAsync<T>(string url, CancellationToken cancellationToken)
        {
            string? continuationToken = null;
            var nextUrl = url;
            var items = new List<T>();
            do
            {
                nextUrl = url +
                    (continuationToken != null ? $"&continuationToken={continuationToken}" : "");
                var response = await _httpClient.GetAsync(nextUrl, cancellationToken);
                response.EnsureSuccessStatusCode();
                var data = await JsonSerializer.DeserializeAsync<Collection<T>>(await response.Content.ReadAsStreamAsync(), JsonSettings.Default);
                items.AddRange(data.Value);

                if (response.Headers.TryGetValues("x-ms-continuationtoken", out var values))
                    continuationToken = values.FirstOrDefault();

            } while (continuationToken != null);

            return items.ToArray();
        }

        private class Collection<T>
        {
            public T[] Value { get; set; } = new T[0];
        }

        private class Build
        {
            public string Name { get; set; } = "";

            public int Id { get; set; }

            public Repository Repository { get; set; } = new Repository();
        }

        private class Repository
        {
            public string Url { get; set; } = "";

            public string Name { get; set; } = "";
        }
    }
}
