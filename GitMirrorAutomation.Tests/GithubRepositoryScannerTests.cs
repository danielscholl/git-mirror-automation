
using FluentAssertions;
using GitMirrorAutomation.Logic.Scanners;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Tests
{
    public class GithubRepositoryScannerTests
    {
        private const string repoUnderTest = "GitMirrorAutomation";
        [Test]
        public void Getting_repository_url_from_repo_name_should_return_with_git_suffix()
        {
            const string githubUrl = "https://github.com/MarcStan";
            var scanner = new GithubRepositoryScanner(githubUrl);
            scanner.GetUrlForRepository("Raytracer").Should().Be($"{githubUrl}/{repoUnderTest}.git");
        }

        [Test]
        public async Task Getting_repositories_from_user_should_return_public_repositories()
        {
            const string githubUrl = "https://github.com/MarcStan";
            var scanner = new GithubRepositoryScanner(githubUrl);
            var repositories = await scanner.GetRepositoriesAsync(CancellationToken.None);
            repositories.Should().Contain(repoUnderTest);
            repositories.Should().HaveCountGreaterOrEqualTo(20);
        }
    }
}
