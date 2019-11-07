using System.Text.Json.Serialization;

namespace GitMirrorAutomation.Logic.Models
{
    public class AzureDevOpsRepository : Repository
    {
        /// <summary>
        /// Correct would be remoteUrl but I prefer without "accountName@" prefix. 
        /// </summary>
        [JsonPropertyName("webUrl")]
        public string GitUrl { get; set; } = "";

        public string Id { get; set; } = "";
    }
}
