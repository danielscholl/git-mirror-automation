using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Scanners
{
    public class GithubRepositoryScanner : IRepositoryScanner
    {
        private static readonly Regex _userRegex = new Regex(@"https:\/\/github\.com\/([^/?&# ]+)");

        private readonly HttpClient _client;
        private readonly string _userName;

        public GithubRepositoryScanner(string userUrl)
        {
            var match = _userRegex.Match(userUrl);
            if (!match.Success)
                throw new ArgumentException("Expected a valid github username url but got: " + userUrl);

            _userName = match.Groups[1].Value;

            _client = new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com/")
            };
            // https://developer.github.com/v3/#current-version
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            // https://developer.github.com/v3/#user-agent-required
            _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitMirrorAutomation", "v1"));
        }

        public async Task<string[]> GetRepositoriesAsync(CancellationToken cancellationToken)
        {
            var repos = new List<string>();
            foreach (var repo in await GetPaginatedAsync<Repo>($"users/{_userName}/repos", cancellationToken))
                repos.Add(repo.Name);

            return repos.ToArray();
        }

        public string GetUrlForRepository(string repository)
            => $"https://github.com/{_userName}/{repository}.git";

        private async Task<T[]> GetPaginatedAsync<T>(string url, CancellationToken cancellationToken)
        {
            string? nextLink = null;
            var items = new List<T>();
            do
            {
                var response = await _client.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                var results = await JsonSerializer.DeserializeAsync<T[]>(await response.Content.ReadAsStreamAsync(), JsonSettings.Default);
                items.AddRange(results);

                if (response.Headers.TryGetValues("Link", out var values))
                {
                    nextLink = values
                        .Select(v =>
                        {
                            // content as per spec: https://developer.github.com/v3/#link-header
                            // Link: <https://api.github.com/user/repos?page=3&per_page=100>; rel="next", < https://api.github.com/user/repos?page=50&per_page=100>; rel="last", ...
                            // only care for rel="next" link

                            if (string.IsNullOrEmpty(v) ||
                                !v.Contains(","))
                                return null;

                            foreach (var hyperlinkSections in v.Split(','))
                            {
                                if (!hyperlinkSections.Contains(";"))
                                    continue;
                                var parts = hyperlinkSections.Split(';');
                                if (!parts[1].Trim().Equals("rel=\"next\"", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                return parts[0].Trim().TrimStart('<').TrimEnd('>');
                            }
                            return null;
                        })
                        .Where(x => x != null)
                        .FirstOrDefault();
                    if (nextLink != null)
                    {
                        url = nextLink;
                        if (url.StartsWith(_client.BaseAddress.ToString(), StringComparison.OrdinalIgnoreCase))
                            url = url.Substring(_client.BaseAddress.ToString().Length);
                    }
                }
            }
            while (nextLink != null);

            return items.ToArray();
        }

        private class Repo
        {
            public string Name { get; set; } = "";
        }
    }
}
