namespace GitMirrorAutomation.Logic.Config
{
    public class MirrorConfig
    {
        public string Target { get; set; } = "";

        public string BuildToClone { get; set; } = "";

        public string BuildNamePrefix { get; set; } = "";

        public AccessToken AccessToken { get; set; } = new AccessToken();
    }
}
