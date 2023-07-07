using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.OperatingSystem
{
    public class OperatingSystemPlugin : IPlugin
    {
        private static readonly string[] Commands = {
            "shutdown", "shut down", "restart", "reboot", "sleep"
        };

        public PluginManifest Manifest { get; set; } = new("Operating System", PluginPriority.Highest)
        {
            Description = "Control your operating system.\n\nExamples: \"sleep\", \"reboot\""
        };

        public PluginStorage Storage { get; set; } = new();

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return query.Parts.Length == 1 && Commands.Contains(query.RawQuery);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            switch (query.RawQuery)
            {
                case "shutdown":
                case "shut down":
                    yield return new ShutdownResult();
                    break;

                case "restart":
                case "reboot":
                    yield return new RestartResult();
                    break;

                case "sleep":
                    yield return new SleepResult();
                    break;
            }
        }

        private class ShutdownResult : AbstractFontAwesomeQueryResult
        {
            public ShutdownResult() : base("Shut down computer")
            {
            }

            public override void OnSelect()
            {
                System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_PowerOff;
        }

        private class RestartResult : AbstractFontAwesomeQueryResult
        {
            public RestartResult() : base("Restart computer")
            {
            }

            public override void OnSelect()
            {
                System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_RedoAlt;
        }

        private class SleepResult : AbstractFontAwesomeQueryResult
        {
            public SleepResult() : base("Sleep/hibernate computer")
            {
            }

            public override void OnSelect()
            {
                Application.SetSuspendState(PowerState.Hibernate, true, true);
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Regular_Moon;
        }
    }
}
