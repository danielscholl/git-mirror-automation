using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Scanners
{
    public interface IRepositorySource
    {
        Task<string[]> GetRepositoriesAsync(CancellationToken cancellationToken);

        string GetUrlForRepository(string repository);

        string Type { get; }
    }
}
