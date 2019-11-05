using FluentAssertions;
using GitMirrorAutomation.Logic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GitMirrorAutomation.Tests
{
    public class ConfigurationProcessorTests
    {
        [Test]
        public void GithubUserSourceSupport()
        {
            var processor = new ConfigurationProcessor(new Mock<ILogger>().Object);
            var scanner = processor.GetRepositoryScanner("https://github.com/MarcStan");
            scanner.Should().NotBeNull();
        }

        [Test]
        public void AzurePipelinesMirrorSupport()
        {
            var processor = new ConfigurationProcessor(new Mock<ILogger>().Object);
            var scanner = processor.GetRepositoryScanner("https://github.com/MarcStan");
            var mirror = processor.GetMirrorService(new Logic.Config.MirrorConfig
            {
                BuildNamePrefix = "[Build]",
                BuildToClone = "[Build] A",
                Target = "https://dev.azure.com/marcstanlive/Opensource"
            }, scanner);
            mirror.Should().NotBeNull();
        }
    }
}
