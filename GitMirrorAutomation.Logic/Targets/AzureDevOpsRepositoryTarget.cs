using GitMirrorAutomation.Logic.Config;
using GitMirrorAutomation.Logic.Helpers;
using GitMirrorAutomation.Logic.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Targets
{
    public class AzureDevOpsRepositoryTarget : AzureDevOpsBase, IRepositoryTarget
    {
        public AzureDevOpsRepositoryTarget(MirrorToConfig mirrorToConfig)
            : base(mirrorToConfig.Target, mirrorToConfig.AccessToken)
        {
        }

        public string Type => "dev.azure.com";

        public async Task CreateRepositoryAsync(IRepository repository, CancellationToken cancellationToken)
        {
            await EnsureAccessToken(cancellationToken);
            var json = JsonSerializer.Serialize(new Repository
            {
                Name = repository.Name
            });
            var response = await HttpClient.PostAsync("git/repositories?api-version=5.1", new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task<IRepository[]> GetRepositoriesAsync(CancellationToken cancellationToken)
        {
            return await GetCollectionAsync<Repository>("git/repositories?api-version=5.1", cancellationToken);
        }

        public string GetUrlForRepository(string repository)
            => $"https://dev.azure.com/{DevOpsAccount}/{DevOpsProject}/_git/{repository}";
    }
}
