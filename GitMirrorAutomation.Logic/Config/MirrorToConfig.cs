namespace GitMirrorAutomation.Logic.Config
{
    public class MirrorToConfig
    {
        public string Target { get; set; } = "";

        public AccessToken AccessToken { get; set; } = new AccessToken();
    }
}
