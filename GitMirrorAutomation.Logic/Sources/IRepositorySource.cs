using GitMirrorAutomation.Logic.Models;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Sources
{
    public interface IRepositorySource
    {
        string Type { get; }

        string SourceId { get; }

        Task<IRepository[]> GetRepositoriesAsync(CancellationToken cancellationToken);

        string GetRepositoryUrl(IRepository repository);
    }
}
