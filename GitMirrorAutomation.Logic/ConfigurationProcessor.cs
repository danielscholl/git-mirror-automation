using GitMirrorAutomation.Logic.Config;
using GitMirrorAutomation.Logic.Mirrors;
using GitMirrorAutomation.Logic.Scanners;
using Microsoft.Extensions.Logging;
using System;

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
    }
}
