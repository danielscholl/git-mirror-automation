using GitMirrorAutomation.Logic.Config;
using GitMirrorAutomation.Logic.Mirrors;
using GitMirrorAutomation.Logic.Sources;
using GitMirrorAutomation.Logic.Targets;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace GitMirrorAutomation.Logic
{
    public class ConfigurationParser
    {
        private readonly ILogger _log;

        public ConfigurationParser(
            ILogger log)
        {
            _log = log;
        }

        public IRepositorySource GetRepositoryScanner(string source)
            => new Uri(source).Host.ToLowerInvariant() switch

            {
                "github.com" => new GithubRepositorySource(source),
                _ => throw new NotSupportedException($"Unsupported source {source}")
            };

        public IMirrorService GetMirrorService(MirrorViaConfig mirrorConfig, IRepositorySource scanner)
            => new Uri(mirrorConfig.Type).Host.ToLowerInvariant() switch
            {
                "dev.azure.com" => new AzurePipelinesMirror(mirrorConfig, scanner, _log),
                _ => throw new NotSupportedException($"Unsupported mirror {mirrorConfig.Type}")
            };

        public IRepositoryTarget[] GetRepositoryTargets(MirrorToConfig[] mirrorToConfig)
        {
            return mirrorToConfig.Select(GetRepositoryTarget).ToArray();
        }

        private IRepositoryTarget GetRepositoryTarget(MirrorToConfig mirrorToConfig)
            => new Uri(mirrorToConfig.Target).Host.ToLowerInvariant() switch
            {
                "gitlab.com" => new GitlabRepositoryTarget(mirrorToConfig),
                "dev.azure.com" => new AzureDevOpsRepositoryTarget(mirrorToConfig),
                _ => throw new NotSupportedException($"Unsupported target {mirrorToConfig.Target}")
            };
    }
}
