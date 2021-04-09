using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Core.Results;
using System.Windows.Forms;
using System.Windows.Input;

namespace Pinpoint.Plugin.ClipboardManager
{
    public class ClipboardResult : AbstractFontAwesomeQueryResult
    {
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

        
        public static void PasteContent()
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                SendKeys.SendWait("^{v}");
            });
        }
    }
}
