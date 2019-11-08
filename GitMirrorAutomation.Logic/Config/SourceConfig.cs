namespace GitMirrorAutomation.Logic.Config
{
    public class SourceConfig
    {
        public string Source { get; set; } = "";

        public AccessToken AccessToken { get; set; } = new AccessToken();

        public string[] ProjectsToIgnore { get; set; } = new string[0];

        public string[] RepositoriesToIgnore { get; set; } = new string[0];
    }
}
