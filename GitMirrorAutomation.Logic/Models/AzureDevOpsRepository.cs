using System.Diagnostics;
using System.Text.Json.Serialization;

namespace GitMirrorAutomation.Logic.Models
{
    [DebuggerDisplay("{DebugString}")]
    public class AzureDevOpsRepository : Repository
    {
        /// <summary>
        /// Correct would be remoteUrl but I prefer without "accountName@" prefix. 
        /// </summary>
        [JsonPropertyName("webUrl")]
        public string GitUrl { get; set; } = "";

        public string Id { get; set; } = "";

        // https://dev.azure.com/{DevOpsAccount}/{DevOpsProject}/_git/{repository.Name}
        public string Project => GitUrl.Split('/')[4];

        public string Organization => GitUrl.Split('/')[3];

        public string DebugString => $"https://dev.azure.com/{Organization}/{Project}/_git/{Name}";
    }
}
