using GitMirrorAutomation.Logic.Config;
using GitMirrorAutomation.Logic.Helpers;
using System;
using System.Linq;
using System.Net.Http;
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
        private string _userName;

        public GitlabRepositoryTarget(
            MirrorToConfig mirrorToConfig)
        {
            _mirrorToConfig = mirrorToConfig;

            var match = _userRegex.Match(_mirrorToConfig.Target);
            if (!match.Success)
                throw new ArgumentException("Expected a valid gitlab username url but got: " + _mirrorToConfig.Target);

            _userName = match.Groups[1].Value;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://gitlab.com/api/v4/")
            };
        }

        public string Type => "Gitlab";

        public Task CreateRepositoryAsync(string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task EnsureAccessToken(CancellationToken cancellationToken)
        {
            if (_httpClient.DefaultRequestHeaders.Authorization != null)
                return;

            var token = await new AccessTokenHelper().GetAsync(_mirrorToConfig.AccessToken, cancellationToken);
            // https://docs.gitlab.com/ee/api/#personal-access-tokens
            _httpClient.DefaultRequestHeaders.Add("Private-Token", token);
        }

        public async Task<string[]> GetRepositoriesAsync(CancellationToken cancellationToken)
        {
            await EnsureAccessToken(cancellationToken);

            return (await _httpClient.GetPaginatedAsync<Project>($"/users/{_userName}/projects", cancellationToken))
                .Select(p => p.Name)
                .ToArray();
        }

        public string GetUrlForRepository(string repository)
            => $"https://gitlab.com/{_userName}/{repository}.git";

        private class Project
        {
            public string Name { get; set; } = "";
        }
    }
}
