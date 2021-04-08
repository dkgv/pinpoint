using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Core.Results;
using System.Windows.Forms;
using System.Windows.Input;

namespace Pinpoint.Plugin.ClipboardManager
{
    public class ClipboardResult : AbstractFontAwesomeQueryResult
    {
        private const byte ControlKey = 0xA2;
        private const byte VKey = 0x56;

        private readonly IClipboardEntry _clipboardEntry;

        public ClipboardResult(IClipboardEntry clipboardEntry) : base(clipboardEntry.Title, "Select to paste")
        {
            _clipboardEntry = clipboardEntry;
        }

        public override void OnSelect()
        {
            switch (_clipboardEntry)
            {
                case TextClipboardEntry textEntry:
                    Clipboard.SetText(textEntry.Content);
                    PasteContent();
                    break;
                case ImageClipboardEntry imageEntry:
                    Clipboard.SetImage(imageEntry.Content);
                    PasteContent();
                    break;
            }
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Regular_Copy;

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public static void PasteContent()
        {
            Task.Run(async () =>
            {
                await Task.Delay(500);
                SendKeys.SendWait("^{v}");
            });
            ////Simulate pressing the keys
            keybd_event(ControlKey, 0, 0, 0);
            keybd_event(VKey, 0, 0, 0);

            ////Simulate releasing the keys
            keybd_event(VKey, 0, 2, 0);
            keybd_event(ControlKey, 0, 2, 0);
        }
    }
}
