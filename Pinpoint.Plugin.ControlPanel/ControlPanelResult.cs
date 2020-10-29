using System.Diagnostics;
using System.Drawing;

namespace Pinpoint.Plugin.ControlPanel
{
    public class ControlPanelResult : AbstractQueryResult
    {
        public ControlPanelResult(string title, Bitmap icon) : base(title, "Show in control panel.")
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