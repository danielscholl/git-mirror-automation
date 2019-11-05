using GitMirrorAutomation.Logic.Sources;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Targets
{
    public interface IRepositoryTarget : IRepositorySource
    {
        Task CreateRepositoryAsync(string name, CancellationToken cancellationToken);
    }
}
