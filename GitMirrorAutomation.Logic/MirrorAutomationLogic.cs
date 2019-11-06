using GitMirrorAutomation.Logic.Mirrors;
using GitMirrorAutomation.Logic.Models;
using GitMirrorAutomation.Logic.Sources;
using GitMirrorAutomation.Logic.Targets;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic
{
    public class MirrorAutomationLogic
    {
        private readonly ILogger _log;

        public MirrorAutomationLogic(
            ILogger log)
        {
            _log = log;
        }

        public async Task ProcessAsync(
            IRepositorySource scanner,
            IMirrorService mirrorService,
            IRepositoryTarget[] targets,
            CancellationToken cancellationToken)
        {
            if (!targets.Any())
                throw new NotSupportedException("At least one target is required!");

            var mirroredRepositories = (await mirrorService.GetExistingMirrorsAsync(cancellationToken))
                .Select(x => x.Repository)
                .ToArray();
            var repos = await scanner.GetRepositoriesAsync(cancellationToken);
            var toMirror = repos.Where(r => !mirroredRepositories.Contains(r.Name)).ToArray();
            if (toMirror.Length == 0)
            {
                _log.LogInformation("All repositories are already mirrored!");
                return;
            }
            _log.LogInformation($"{repos.Length - toMirror.Length} repositories are already mirrored, setting up mirrors for the remaining {toMirror.Length} repositories..");
            foreach (var repo in toMirror)
            {
                _log.LogInformation($"Creating mirror for repository {repo}");
                // create mirror last as it is the marker whether a repo mirror already exists
                // creating repositories is also idempotent
                foreach (var target in targets)
                {
                    var existing = await target.GetRepositoriesAsync(cancellationToken);
                    if (existing.Any(e => e.Name == repo.Name))
                        return;

                    await target.CreateRepositoryAsync(repo, cancellationToken);
                }
                await mirrorService.SetupMirrorAsync(repo, cancellationToken);
                _log.LogInformation($"Created mirror for repository {repo}");
            }
        }
    }
}
