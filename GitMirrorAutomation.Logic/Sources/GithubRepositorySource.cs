using GitMirrorAutomation.Logic.Helpers;
using GitMirrorAutomation.Logic.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Sources
{
    public class GithubRepositorySource : IRepositorySource
    {
        private readonly HttpClient _httpClient;
        private readonly string _paginationTemplate;

        public GithubRepositorySource(string userName, string paginationTemplate)
        {
            UserName = userName;
            _paginationTemplate = paginationTemplate;

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

        public async Task<IRepository[]> GetRepositoriesAsync(CancellationToken cancellationToken)
        {
            return await _httpClient.GetPaginatedAsync<GithubRepository>(string.Format(_paginationTemplate, UserName), cancellationToken);
        }

        public string GetRepositoryUrl(string repository)
            => $"https://github.com/{UserName}/{repository}.git";
    }
}
