using System.Diagnostics;
using FontAwesome5;

namespace Pinpoint.Plugin.CommandLine
{
    public class CommandLineResult : IQueryResult
    {
        public CommandLineResult(string cmd)
        {
            Title = cmd.Substring(1);
            Subtitle = "Execute in command prompt.";
        }

        public string Title { get; }

        public string Subtitle { get; }

        public object Instance { get; }

        public EFontAwesomeIcon Icon => EFontAwesomeIcon.Solid_Terminal;

        public void OnSelect()
        {
            Process.Start("cmd.exe", "/k" + Title);
        }
    }
}