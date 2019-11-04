using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Mirrors
{
    public interface IMirrorService
    {
        Task<Mirror[]> GetExistingMirrorsAsync(CancellationToken cancellationToken);

        Task SetupMirrorAsync(string repository, CancellationToken cancellationToken);
    }
}
