using GitMirrorAutomation.Logic.Config;
using GitMirrorAutomation.Logic.Mirrors;
using GitMirrorAutomation.Logic.Scanners;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic
{
    public class ConfigurationProcessor
    {
        private readonly ILogger _log;

        public ConfigurationProcessor(
            ILogger log)
        {
            _log = log;
        }

        public async Task ProcessAsync(Configuration cfg, CancellationToken cancellationToken)
        {
            var scanner = GetRepositoryScanner(cfg.Source);
            var mirrorService = GetMirrorService(cfg.MirrorConfig, scanner);
            var mirroredRepositories = new List<string>();
            foreach (var mirror in await mirrorService.GetExistingMirrorsAsync(cancellationToken))
            {
                mirroredRepositories.Add(mirror.Repository);
            }
            var repos = await scanner.GetRepositoriesAsync(cancellationToken);
            var toMirror = repos.Where(r => !mirroredRepositories.Contains(r)).ToArray();
            if (toMirror.Length == 0)
            {
                _log.LogInformation("All repositories are already mirrored!");
                return;
            }
            _log.LogInformation($"{repos.Length - toMirror.Length} repositories are already mirrored, setting up mirrors for the remaining {toMirror.Length} repositories..");
            foreach (var repo in repos)
            {
                _log.LogInformation($"Creating mirror for repository {repo}");
                await mirrorService.SetupMirrorAsync(repo, cancellationToken);
                _log.LogInformation($"Created mirror for repository {repo}");
            }
        }

        private IRepositoryScanner GetRepositoryScanner(string source)
            => new Uri(source).Host.ToLowerInvariant() switch

            {
                "github.com" => new GithubRepositorScanner(source),
                _ => throw new NotSupportedException($"Unsupported source {source}")
            };

        private IMirrorService GetMirrorService(MirrorConfig mirrorConfig, IRepositoryScanner scanner)
            => new Uri(mirrorConfig.Target).Host.ToLowerInvariant() switch
            {
                "dev.azure.com" => new AzurePipelinesMirror(mirrorConfig, scanner, _log),
                _ => throw new NotSupportedException($"Unsupported mirror {mirrorConfig.Target}")
            };
    }
}
