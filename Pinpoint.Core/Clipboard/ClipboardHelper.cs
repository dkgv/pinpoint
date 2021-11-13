using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Core.Clipboard
{
    public static class ClipboardHelper
    {
        public static readonly ClipboardHistory History = new();
        
        public static void PrependToHistory(IClipboardEntry entry)
        {
            var icon = NativeProvider.GetActiveWindowIcon();
            entry.Icon = icon ?? FontAwesomeBitmapRepository.Get(EFontAwesomeIcon.Regular_Copy);

            History.AddFirst(entry);
        }

        public static void PasteClipboard()
        {
            _ = Task.Run(async () =>
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
