using GitMirrorAutomation.Logic.Config;
using GitMirrorAutomation.Logic.Helpers;
using GitMirrorAutomation.Logic.Models;
using GitMirrorAutomation.Logic.Sources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Mirrors
{
    public class AzurePipelinesMirror : AzureDevOpsBase, IMirrorService
    {
        private readonly MirrorViaConfig _config;
        private int? _buildToCloneId;
        private readonly IRepositorySource _repositorySource;
        private readonly ILogger _log;

        public AzurePipelinesMirror(
            MirrorViaConfig config,
            IRepositorySource repositoryScanner,
            ILogger log)
            : base(config.Type, config.AccessToken)
        {
            _config = config;
            _repositorySource = repositoryScanner;
            _log = log;
        }

        public async Task<Mirror[]> GetExistingMirrorsAsync(CancellationToken cancellationToken)
        {
            var mirrors = new List<Mirror>();
            var builds = await GetBuildsAsync(cancellationToken);
            var mirrorBuilds = builds.Where(b => b.Name.StartsWith(_config.BuildNamePrefix)).ToArray();
            _log.LogInformation($"Looking for existing mirrors in {builds.Length} builds ({mirrorBuilds.Length} of those are mirror builds)..");
            await EnsureAccessToken(cancellationToken);
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

        public async Task SetupMirrorAsync(IRepository repository, CancellationToken cancellationToken)
        {
            if (!_buildToCloneId.HasValue)
                throw new InvalidOperationException($"Must call {nameof(GetExistingMirrorsAsync)} first before setting up a new mirror!");

            await EnsureAccessToken(cancellationToken);

            var buildDefinition = await GetBuildDefinitionAsync(_buildToCloneId.Value, cancellationToken);
            // real inefficient but there seems to be no way to modify a JsonElement + this usually isn't executed a million times..
            var jObject = JObject.Parse(buildDefinition.GetRawText());
            jObject["repository"]["url"] = _repositorySource.GetUrlForRepository(repository.Name);

            if (_repositorySource.Type != "github.com")
                throw new NotSupportedException("Currently only github is a supported repository source!");

            // id is needed and seems to be dependent on type of source
            jObject["repository"]["id"] = "Github/" + repository;
            jObject["repository"]["name"] = "Github/" + repository;
            jObject["name"] = _config.BuildNamePrefix + repository;

            var json = jObject.ToString();
            var response = await HttpClient.PostAsync("build/definitions?api-version=5.1", new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        private async Task<JsonElement> GetBuildDefinitionAsync(int id, CancellationToken cancellationToken)
        {
            var response = await HttpClient.GetAsync($"build/definitions/{id}?api-version=5.1");
            response.EnsureSuccessStatusCode();
            return await JsonSerializer.DeserializeAsync<JsonElement>(await response.Content.ReadAsStreamAsync(), JsonSettings.Default, cancellationToken);
        }

        private Task<Build[]> GetBuildsAsync(CancellationToken cancellationToken)
            => GetCollectionAsync<Build>("build/definitions?api-version=5.1", cancellationToken);

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
