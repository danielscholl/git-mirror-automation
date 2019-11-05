
using FluentAssertions;
using GitMirrorAutomation.Logic.Sources;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Tests
{
    public class GithubRepositoryScannerTests
    {
        private const string _repoUnderTest = "GitMirrorAutomation";
        private const string _userUnderTest = "MarcStan";

        [Test]
        public void Getting_repository_url_from_repo_name_should_return_with_git_suffix()
        {
            var githubUrl = $"https://github.com/{_userUnderTest}";
            var scanner = new GithubRepositorySource(githubUrl);
            scanner.GetUrlForRepository(_repoUnderTest).Should().Be($"{githubUrl}/{_repoUnderTest}.git");
        }

        [Test]
        public async Task Getting_repositories_from_user_should_return_public_repositories()
        {
            var githubUrl = $"https://github.com/{_userUnderTest}";
            var scanner = new GithubRepositorySource(githubUrl);
            var repositories = await scanner.GetRepositoriesAsync(CancellationToken.None);
            repositories.Should().Contain(_repoUnderTest);
            repositories.Should().HaveCountGreaterOrEqualTo(20);
        }
    }
}
