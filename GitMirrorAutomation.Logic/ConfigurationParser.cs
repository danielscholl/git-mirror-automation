using GitMirrorAutomation.Logic.Config;
using GitMirrorAutomation.Logic.Mirrors;
using GitMirrorAutomation.Logic.Sources;
using GitMirrorAutomation.Logic.Targets;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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

        public IRepositorySource GetRepositorySource(string source)
            => new Uri(source).Host.ToLowerInvariant() switch

            {
                "github.com" => ParseGithubSource(source),
                _ => throw new NotSupportedException($"Unsupported source {source}")
            };

        public IMirrorService GetMirrorService(MirrorViaConfig mirrorConfig, IRepositorySource source)
            => new Uri(mirrorConfig.Type).Host.ToLowerInvariant() switch
            {
                "dev.azure.com" => new AzurePipelinesMirror(mirrorConfig, source, _log),
                _ => throw new NotSupportedException($"Unsupported mirror {mirrorConfig.Type}")
            };

        public IRepositoryTarget[] GetRepositoryTargets(MirrorToConfig[] mirrorToConfig)
        {
            return mirrorToConfig.Select(GetRepositoryTarget).ToArray();
        }

        private IRepositorySource ParseGithubSource(string source)
        {
            var starRegex = new Regex(@"https:\/\/github\.com\/([^/?&# ]+)/starred");
            var match = starRegex.Match(source);
            if (match.Success)
                return new GithubRepositorySource(match.Groups[1].Value, "users/{0}/starred");

            var userRegex = new Regex(@"https:\/\/github\.com\/([^/?&# ]+)");
            match = userRegex.Match(source);
            if (match.Success)
                return new GithubRepositorySource(match.Groups[1].Value, "users/{0}/repos");

            throw new ArgumentException("Expected a valid github username url but got: " + source);
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
