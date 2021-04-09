using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pinpoint.Core
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

        public static void Copy(string content) => Clipboard.SetText(content);
    }
}
