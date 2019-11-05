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

        public IRepositoryScanner GetRepositoryScanner(string source)
            => new Uri(source).Host.ToLowerInvariant() switch

            {
                "github.com" => new GithubRepositoryScanner(source),
                _ => throw new NotSupportedException($"Unsupported source {source}")
            };

        public IMirrorService GetMirrorService(MirrorConfig mirrorConfig, IRepositoryScanner scanner)
            => new Uri(mirrorConfig.Target).Host.ToLowerInvariant() switch
            {
                "dev.azure.com" => new AzurePipelinesMirror(mirrorConfig, scanner, _log),
                _ => throw new NotSupportedException($"Unsupported mirror {mirrorConfig.Target}")
            };
    }
}
