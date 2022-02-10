using System.Diagnostics;
using System.Drawing;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.AppSearch;

namespace Pinpoint.Plugin.UWPSearch
{
    public class UwpAppResult : AbstractQueryResult
    {
        private readonly string _shellExecution;
        public UwpAppResult(string appValue) : base(appValue.Split("#")[0], "UWP App")
        {
            _shellExecution = appValue.Split("#")[1];
            Options.Add(new RunAsAdminOption(_shellExecution));
        }

        //TODO: Figure out how to get icon for app.
        public override Bitmap Icon => System.Drawing.Icon.ExtractAssociatedIcon("")?.ToBitmap();

        public override void OnSelect()
        {
            AppSearchFrequency.Track(UwpSearchPlugin.LastQuery, _shellExecution);
            Process.Start("explorer.exe", $"shell:AppsFolder\\{_shellExecution}");
        }

        public override bool OnPrimaryOptionSelect()
        {
            Process.Start("explorer.exe", $"shell:AppsFolder\\{_shellExecution}");
            return true;
        }
    }
}