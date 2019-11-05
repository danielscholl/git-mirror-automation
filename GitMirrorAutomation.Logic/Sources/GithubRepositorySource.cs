using GitMirrorAutomation.Logic.Helpers;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Sources
{
    public class GithubRepositorySource : IRepositorySource
    {
        private static readonly Regex _userRegex = new Regex(@"https:\/\/github\.com\/([^/?&# ]+)");

        private readonly HttpClient _httpClient;
        private readonly string _userName;

        public GithubRepositorySource(string userUrl)
        {
            var match = _userRegex.Match(userUrl);
            if (!match.Success)
                throw new ArgumentException("Expected a valid github username url but got: " + userUrl);

            _userName = match.Groups[1].Value;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com/")
            };
            // https://developer.github.com/v3/#current-version
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            // https://developer.github.com/v3/#user-agent-required
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitMirrorAutomation", "v1"));
        }

        public string Type => "Github";

        public async Task<string[]> GetRepositoriesAsync(CancellationToken cancellationToken)
        {
            return (await _httpClient.GetPaginatedAsync<Repo>($"users/{_userName}/repos", cancellationToken))
                .Select(r => r.Name)
                .ToArray();
        }

        public string GetUrlForRepository(string repository)
            => $"https://github.com/{_userName}/{repository}.git";


        private class Repo
        {
            public string Name { get; set; } = "";
        }
    }
}
