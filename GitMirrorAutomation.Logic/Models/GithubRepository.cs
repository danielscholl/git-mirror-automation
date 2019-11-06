using System.Text.Json.Serialization;

namespace GitMirrorAutomation.Logic.Models
{
    public class GithubRepository : Repository
    {
        [JsonPropertyName("git_url")]
        public string GitUrl { get; set; } = "";
    }
}
