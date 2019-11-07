using GitMirrorAutomation.Logic.Models;
using GitMirrorAutomation.Logic.Sources;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Targets
{
    public interface IRepositoryTarget : IRepositorySource
    {
        string TargetId { get; }

        Task CreateRepositoryAsync(IRepository repository, CancellationToken cancellationToken);
    }
}
