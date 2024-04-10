using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.OperatingSystem
{
    public class OperatingSystemPlugin : AbstractPlugin
    {
        private static readonly string[] Commands = {
            "shutdown", "shut down", "restart", "reboot", "sleep", "emptytrash", "emptybin", "trash", "bin"
        };

        public override PluginManifest Manifest { get; } = new("Operating System", PluginPriority.High)
        {
            Description = "Control your operating system.\n\nExamples: \"sleep\", \"reboot\""
        };

        public override async Task<bool> ShouldActivate(Query query)
        {
            return query.Parts.Length == 1 && Commands.Contains(query.Raw);
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            switch (query.Raw)
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

                case "emptytrash":
                case "emptybin":
                    yield return new EmptyTrashResult();
                    break;

                case "trash":
                case "bin":
                    yield return new RecycleBin();
                    break;
            }
        }

        private class RecycleBin : AbstractFontAwesomeQueryResult
        {
            public RecycleBin() : base("Open recycle bin")
            {
            }

            public override void OnSelect()
            {
                System.Diagnostics.Process.Start("explorer.exe", "shell:RecycleBinFolder");
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_TrashAlt;
        }

        private class EmptyTrashResult : AbstractFontAwesomeQueryResult
        {
            public EmptyTrashResult() : base("Empty recycle bin")
            {
            }

            public override void OnSelect()
            {
                System.Diagnostics.Process.Start("cmd.exe", "/c rd /s /q C:\\$Recycle.Bin");
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_TrashAlt;
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
