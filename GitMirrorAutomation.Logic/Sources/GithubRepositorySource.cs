using GitMirrorAutomation.Logic.Helpers;
using GitMirrorAutomation.Logic.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Sources
{
    public class GithubRepositorySource : IRepositorySource
    {
        private readonly Regex _userRegex = new Regex(@"https:\/\/github\.com\/([^/?&# ]+)$");

        private readonly HttpClient _httpClient;

        public GithubRepositorySource(string userUrl)
        {
            var match = _userRegex.Match(userUrl);
            if (!match.Success)
                throw new ArgumentException("Expected a valid github username url but got: " + userUrl);

            UserName = match.Groups[1].Value;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com/")
            };
            // https://developer.github.com/v3/#current-version
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            // https://developer.github.com/v3/#user-agent-required
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitMirrorAutomation", "v1"));
        }

        public string UserName { get; }

        public string Type => "github.com";

        public string SourceId => $"{Type}/{UserName}";

        public async Task<IRepository[]> GetRepositoriesAsync(CancellationToken cancellationToken)
        {
            return await _httpClient.GetPaginatedAsync<GithubRepository>($"users/{UserName}/repos", cancellationToken);
        }

        public string[] GetRepositoryUrls(IRepository repository)
            => new[]
            {
                $"https://github.com/{UserName}/{repository.Name}",
                $"https://github.com/{UserName}/{repository.Name}.git"
            };
    }
}
