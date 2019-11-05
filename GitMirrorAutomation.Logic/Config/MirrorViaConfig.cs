namespace GitMirrorAutomation.Logic.Config
{
    public class MirrorViaConfig
    {
        public string Type { get; set; } = "";

        public string BuildToClone { get; set; } = "";

        public string BuildNamePrefix { get; set; } = "";

        public AccessToken AccessToken { get; set; } = new AccessToken();
    }
}
