using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitMirrorAutomation.Logic.Config
{
    public class Configuration
    {
        public JsonElement Source { get; set; } = new JsonElement();

        [JsonPropertyName("mirror-via")]
        public MirrorViaConfig MirrorViaConfig { get; set; } = new MirrorViaConfig();

        [JsonPropertyName("mirror-to")]
        public TargetConfig[] TargetConfig { get; set; } = new TargetConfig[0];
    }
}
