using GitMirrorAutomation.Logic;
using GitMirrorAutomation.Logic.Mirrors;
using GitMirrorAutomation.Logic.Scanners;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Tests
{
    public class MirrorAutomationLogicTests
    {
        [Test]
        public async Task When_no_repositories_exist_should_not_mirror_anything()
        {
            var automation = new MirrorAutomationLogic(new Mock<ILogger>().Object);
            var scanner = new Mock<IRepositoryScanner>();
            scanner.Setup(x => x.GetRepositoriesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new string[0]));
            var mirror = new Mock<IMirrorService>();
            mirror.Setup(x => x.GetExistingMirrorsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Mirror[0]));
            await automation.ProcessAsync(scanner.Object, mirror.Object, CancellationToken.None);

            mirror.Verify(x => x.SetupMirrorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task When_repositories_exist_and_they_are_not_mirrored_Then_should_mirror()
        {
            var automation = new MirrorAutomationLogic(new Mock<ILogger>().Object);
            var scanner = new Mock<IRepositoryScanner>();
            scanner.Setup(x => x.GetRepositoriesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new[]
                    {
                        "Repo1",
                        "Repo2"
                    }));
            var mirror = new Mock<IMirrorService>();
            mirror.Setup(x => x.GetExistingMirrorsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Mirror[0]));
            await automation.ProcessAsync(scanner.Object, mirror.Object, CancellationToken.None);

            mirror.Verify(x => x.SetupMirrorAsync("Repo1", It.IsAny<CancellationToken>()), Times.Once);
            mirror.Verify(x => x.SetupMirrorAsync("Repo2", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task When_repositories_exist_but_are_mirrored_Then_should_not_mirror_again()
        {
            var automation = new MirrorAutomationLogic(new Mock<ILogger>().Object);
            var scanner = new Mock<IRepositoryScanner>();
            scanner.Setup(x => x.GetRepositoriesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new[]
                    {
                        "Repo1",
                        "Repo2"
                    }));
            var mirror = new Mock<IMirrorService>();
            mirror.Setup(x => x.GetExistingMirrorsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new[]
                {
                    new Mirror
                    {
                        Repository = "Repo1"
                    },
                    new Mirror
                    {
                        Repository = "Repo2"
                    }
                }));
            await automation.ProcessAsync(scanner.Object, mirror.Object, CancellationToken.None);

            mirror.Verify(x => x.SetupMirrorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
