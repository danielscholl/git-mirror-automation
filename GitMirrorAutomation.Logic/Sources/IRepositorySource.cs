using GitMirrorAutomation.Logic.Models;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Sources
{
    public interface IRepositorySource
    {
        string Type { get; }

        Task<IRepository[]> GetRepositoriesAsync(CancellationToken cancellationToken);

        string GetRepositoryUrl(string repository);
    }
}
