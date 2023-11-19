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
                  await Task.Delay(50);
                  SendKeys.SendWait("^{v}");
              });
        }

        public static void Copy(string content)
        {
            NativeProvider.Copy(content);
        }
    }
}
