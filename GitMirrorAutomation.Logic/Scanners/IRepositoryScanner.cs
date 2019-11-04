using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Scanners
{
    public interface IRepositoryScanner
    {
        Task<string[]> GetRepositoriesAsync(CancellationToken cancellationToken);

        string GetUrlForRepository(string repository);
    }
}
