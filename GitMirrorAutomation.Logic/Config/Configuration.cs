using System.Text.Json.Serialization;

namespace GitMirrorAutomation.Logic.Config
{
    public class Configuration
    {
        public string Source { get; set; } = "";

        [JsonPropertyName("mirror-via")]
        public MirrorViaConfig MirrorViaConfig { get; set; } = new MirrorViaConfig();

        [JsonPropertyName("mirror-to")]
        public MirrorToConfig[] MirrorToConfig { get; set; } = new MirrorToConfig[0];
    }
}
