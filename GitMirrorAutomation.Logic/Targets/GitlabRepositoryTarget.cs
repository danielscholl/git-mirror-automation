using GitMirrorAutomation.Logic.Config;
using GitMirrorAutomation.Logic.Helpers;
using GitMirrorAutomation.Logic.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Targets
{
    public class GitlabRepositoryTarget : IRepositoryTarget
    {
        private static readonly Regex _userRegex = new Regex(@"https:\/\/gitlab\.com\/([^/?&# ]+)");

        private readonly MirrorToConfig _mirrorToConfig;
        private readonly HttpClient _httpClient;

        public GitlabRepositoryTarget(
            MirrorToConfig mirrorToConfig)
        {
            _mirrorToConfig = mirrorToConfig;

            var match = _userRegex.Match(_mirrorToConfig.Target);
            if (!match.Success)
                throw new ArgumentException("Expected a valid gitlab username url but got: " + _mirrorToConfig.Target);

            UserName = match.Groups[1].Value;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://gitlab.com/api/v4/")
            };
        }

        public string UserName { get; }

        public string Type => "gitlab.com";

        public string SourceId => $"{Type}/{UserName}";
        public string TargetId => SourceId;

        public async Task CreateRepositoryAsync(IRepository repository, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(new
            {
                // must set path, if setting name then path will be lowerecased
                // https://docs.gitlab.com/ee/api/projects.html#create-project
                path = repository.Name,
                visibility = "public",
                description = repository.Description
            });
            var response = await _httpClient.PostAsync("projects", new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        private async Task EnsureAccessToken(CancellationToken cancellationToken)
        {
            if (_httpClient.DefaultRequestHeaders.Authorization != null)
                return;

            var token = await new AccessTokenHelper().GetAsync(_mirrorToConfig.AccessToken, cancellationToken);
            // https://docs.gitlab.com/ee/api/#personal-access-tokens
            _httpClient.DefaultRequestHeaders.Add("Private-Token", token);
        }

        public async Task<IRepository[]> GetRepositoriesAsync(CancellationToken cancellationToken)
        {
            await EnsureAccessToken(cancellationToken);

            return (await _httpClient.GetPaginatedAsync<GitlabRepository>($"users/{UserName}/projects", cancellationToken))
                .ToArray();
        }

        public string GetRepositoryUrl(IRepository repository)
            => repository is GitlabRepository glRepo ? glRepo.GitUrl : $"https://gitlab.com/{UserName}/{repository.Name}.git";
    }
}
