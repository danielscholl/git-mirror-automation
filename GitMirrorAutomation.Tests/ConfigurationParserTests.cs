using FluentAssertions;
using GitMirrorAutomation.Logic;
using GitMirrorAutomation.Logic.Sources;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GitMirrorAutomation.Tests
{
    public class ConfigurationParserTests
    {
        [Test]
        public void GithubUserSourceSupport()
        {
            var processor = new ConfigurationParser(new Mock<ILogger>().Object);
            var source = processor.GetRepositorySource("https://github.com/MarcStan");
            source.Should().BeOfType<GithubRepositorySource>();
        }

        [Test]
        public void GithubUserStarsSourceSupport()
        {
            var processor = new ConfigurationParser(new Mock<ILogger>().Object);
            var source = processor.GetRepositorySource("https://github.com/MarcStan/starred");
            source.Should().BeOfType<GithubRepositorySource>();
        }

        [Test]
        public void AzurePipelinesMirrorSupport()
        {
            var processor = new ConfigurationParser(new Mock<ILogger>().Object);
            var source = processor.GetRepositorySource("https://github.com/MarcStan");
            var mirror = processor.GetMirrorService(new Logic.Config.MirrorViaConfig
            {
                BuildNamePrefix = "[Build]",
                BuildToClone = "[Build] A",
                Type = "https://dev.azure.com/marcstanlive/Opensource"
            }, source);
            mirror.Should().NotBeNull();
        }
    }
}
