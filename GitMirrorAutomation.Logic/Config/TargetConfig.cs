namespace GitMirrorAutomation.Logic.Config
{
    public class TargetConfig
    {
        public string Target { get; set; } = "";

        public AccessToken AccessToken { get; set; } = new AccessToken();
    }
}
