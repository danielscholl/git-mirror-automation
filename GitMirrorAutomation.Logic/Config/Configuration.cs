using System.Text.Json.Serialization;

namespace GitMirrorAutomation.Logic.Config
{
    public class Configuration
    {
        public string Source { get; set; } = "";


        [JsonPropertyName("mirror-via")]
        public MirrorConfig MirrorConfig { get; set; } = new MirrorConfig();
    }
}
