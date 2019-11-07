using FluentAssertions;
using GitMirrorAutomation.Logic;
using GitMirrorAutomation.Logic.Sources;
using GitMirrorAutomation.Logic.Targets;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Text.Json;

namespace GitMirrorAutomation.Tests
{
    public class ConfigurationParserTests
    {
        [Test]
        public void GithubUserSourceSupport()
        {
            var processor = new ConfigurationParser(new Mock<ILogger>().Object);
            var source = processor.GetRepositorySource(ToJson("https://github.com/MarcStan"));
            source.Should().BeOfType<GithubRepositorySource>();
        }

        [Test]
        public void AzureDevOpsSourceSupport()
        {
            var processor = new ConfigurationParser(new Mock<ILogger>().Object);
            var source = processor.GetRepositorySource(ToJson(new
            {
                source = "https://dev.azure.com/marcstanlive/Opensoure",
                accessToken = new
                {
                    source = "https://mykeyvault.vault.azure.net",
                    secretName = "MyDevOpsGitPAT"
                }
            }));
            source.Should().BeOfType<AzureDevOpsRepositoryTarget>();
        }

        [Test]
        public void AzurePipelinesMirrorSupport()
        {
            var processor = new ConfigurationParser(new Mock<ILogger>().Object);
            var source = processor.GetRepositorySource(ToJson("https://github.com/MarcStan"));
            var mirror = processor.GetMirrorService(new Logic.Config.MirrorViaConfig
            {
                BuildNamePrefix = "[Build]",
                BuildToClone = "[Build] A",
                Type = "https://dev.azure.com/marcstanlive/Opensource"
            }, source);
            mirror.Should().NotBeNull();
        }

        private JsonElement ToJson<T>(T value)
            => JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement;
    }
}
