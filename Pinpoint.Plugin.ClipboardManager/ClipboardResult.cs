using FontAwesome5;
using Pinpoint.Core.Results;

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
            
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Regular_Copy;
    }
}
