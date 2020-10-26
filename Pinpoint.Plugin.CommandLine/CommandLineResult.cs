using System.Diagnostics;
using FontAwesome5;

namespace Pinpoint.Plugin.CommandLine
{
    public class CommandLineResult : AbstractQueryResult
    {
        public CommandLineResult(string cmd) : base(cmd.Substring(1), "Execute in command prompt.")
        {
        }

        public override EFontAwesomeIcon Icon => EFontAwesomeIcon.Solid_Terminal;

        public override void OnSelect()
        {
            Process.Start("cmd.exe", "/k" + Title);
        }
    }
}