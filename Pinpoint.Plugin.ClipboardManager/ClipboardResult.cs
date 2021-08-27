using FontAwesome5;
using Pinpoint.Core.Results;
using System.Windows.Forms;
using Pinpoint.Core;
using Pinpoint.Core.Clipboard;

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
                    break;
                
                case ImageClipboardEntry imageEntry:
                    Clipboard.SetImage(imageEntry.Content);
                    break;
            }

            ClipboardHelper.PasteClipboard();
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Regular_Copy;
    }
}
