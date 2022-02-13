using System.Diagnostics;
using System.Drawing;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.ControlPanel
{
    public class ControlPanelResult : AbstractQueryResult
    {
        public ControlPanelResult(string title, string description, Bitmap icon) : base(title, description)
        {
            Icon = icon;
        }

        public override Bitmap Icon { get; }

        public override void OnSelect()
        {
            const string args = "-NonInteractive -NoProfile -Command \"Show-ControlPanelItem -Name '{0}'\"";
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "powershell",
                    Arguments = string.Format(args, Title),
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
        }
    }
}