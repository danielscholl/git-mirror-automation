
using FluentAssertions;
using GitMirrorAutomation.Logic.Models;
using GitMirrorAutomation.Logic.Sources;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Tests
{
    public class GithubRepositorySourceTests
    {
        private const string _repoUnderTest = "GitMirrorAutomation";
        private const string _userUnderTest = "MarcStan";

        [Test]
        public void Getting_repository_url_from_repo_name_should_return_with_git_suffix()
        {
            var githubUrl = $"https://github.com/{_userUnderTest}";
            var scanner = new GithubRepositorySource(_userUnderTest, "users/{0}/repos");
            scanner.GetRepositoryUrl(new Repository
            {
                Name = _repoUnderTest
            }).Should().Be($"{githubUrl}/{_repoUnderTest}.git");
        }

        [Test]
        public void Getting_repository_url_from_github_repo_name_should_return_with_git_suffix()
        {
            var githubUrl = $"https://github.com/{_userUnderTest}";
            var scanner = new GithubRepositorySource(_userUnderTest, "users/{0}/repos");
            scanner.GetRepositoryUrl(new GithubRepository
            {
                Name = _repoUnderTest,
                GitUrl = "prefferredValue"
            }).Should().Be("prefferredValue");
        }

        [Test]
        public async Task Getting_repositories_from_user_should_return_public_repositories()
        {
            var githubUrl = $"https://github.com/{_userUnderTest}";
            var scanner = new GithubRepositorySource(_userUnderTest, "users/{0}/repos");
            var repositories = await scanner.GetRepositoriesAsync(CancellationToken.None);
            repositories.SingleOrDefault(r => r.Name == _repoUnderTest).Should().NotBeNull();
            repositories.Should().HaveCountGreaterOrEqualTo(20);
        }

        [Test]
        public async Task Getting_starred_repositories_from_user_should_return_starred_repositories()
        {
            var githubUrl = $"https://github.com/{_userUnderTest}";
            var scanner = new GithubRepositorySource(_userUnderTest, "users/{0}/starred");
            var repositories = await scanner.GetRepositoriesAsync(CancellationToken.None);
            repositories.SingleOrDefault(r => r.Name == "wpf").Should().NotBeNull();
            repositories.Should().HaveCountGreaterOrEqualTo(20);
        }
    }
}
