using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pinpoint.Core.Clipboard
{
    public static class ClipboardHelper
    {
        public static void PasteClipboard()
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                SendKeys.SendWait("^{v}");
            });
        }

        public static void Copy(string content)
        {
            var clipboardExecutable = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardInput = true,
                    FileName = @"clip",
                }
            };
            clipboardExecutable.Start();
            clipboardExecutable.StandardInput.Write(content);
            clipboardExecutable.StandardInput.Close();
        }
    }
}
