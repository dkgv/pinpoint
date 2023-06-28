using System.Diagnostics;
using System.Text;
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
                  await Task.Delay(150);
                  SendKeys.SendWait("^{v}");
              });
        }

        public static void CopyUtf8(string content)
        {
            Copy(content, Encoding.UTF8);
        }

        public static void Copy(string content, Encoding encoding)
        {
            var clipboardExecutable = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardInput = true,
                    StandardInputEncoding = encoding,
                    FileName = @"clip",
                }
            };
            clipboardExecutable.Start();
            clipboardExecutable.StandardInput.Write(content);
            clipboardExecutable.StandardInput.Close();
            clipboardExecutable.WaitForExit();
        }
    }
}
