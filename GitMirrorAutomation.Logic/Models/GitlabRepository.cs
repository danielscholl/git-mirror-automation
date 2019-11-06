using System.Text.Json.Serialization;

namespace GitMirrorAutomation.Logic.Models
{
    public class GitlabRepository : Repository
    {
        [JsonPropertyName("http_url_to_repo")]
        public string GitUrl { get; set; } = "";
    }
}
