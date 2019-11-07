using GitMirrorAutomation.Logic.Models;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Mirrors
{
    public interface IMirrorService
    {
        string Mirror { get; }

        Task<Mirror[]> GetExistingMirrorsAsync(CancellationToken cancellationToken);

        Task SetupMirrorAsync(IRepository repository, CancellationToken cancellationToken);
    }
}
