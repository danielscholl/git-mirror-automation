
using System.Text.Json;

namespace GitMirrorAutomation.Logic
{
    public static class JsonSettings
    {
        public static readonly JsonSerializerOptions Default;

        static JsonSettings()
        {
            Default = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
        }
    }
}
