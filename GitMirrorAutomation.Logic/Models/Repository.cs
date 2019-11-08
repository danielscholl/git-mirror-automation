using System.Diagnostics;

namespace GitMirrorAutomation.Logic.Models
{
    [DebuggerDisplay("{Name}")]
    public class Repository : IRepository
    {
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";
    }
}
