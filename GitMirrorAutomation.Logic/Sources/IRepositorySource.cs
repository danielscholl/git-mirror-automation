using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Sources
{
    public interface IRepositorySource
    {
        Task<string[]> GetRepositoriesAsync(CancellationToken cancellationToken);

        string GetUrlForRepository(string repository);

        string Type { get; }
    }
}
