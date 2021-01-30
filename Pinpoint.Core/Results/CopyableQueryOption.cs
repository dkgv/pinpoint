using System.Diagnostics;
using System.Drawing;

namespace Pinpoint.Core.Results
{
    public class CopyabableQueryOption : AbstractQueryResult
    {
        private readonly string _clipboardContent;

        public CopyabableQueryOption(string title, string subtitle, Bitmap icon, string clipboardContent) : base(title, subtitle)
        {
            _clipboardContent = clipboardContent;
            Icon = icon;
        }

        public override Bitmap Icon { get; }

        public override void OnSelect()
        {
            var clipboardExecutable = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardInput = true, FileName = @"clip",
                }
            };
            clipboardExecutable.Start();
            clipboardExecutable.StandardInput.Write(_clipboardContent);
            clipboardExecutable.StandardInput.Close();
        }
    }
}
