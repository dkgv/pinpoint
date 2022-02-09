using System;
using System.Drawing;
using Pinpoint.Core.Results;
using System.Windows.Forms;
using Pinpoint.Core.Clipboard;

namespace Pinpoint.Plugin.ClipboardManager
{
    public class ClipboardResult : AbstractQueryResult
    {
        private readonly IClipboardEntry _clipboardEntry;

        public ClipboardResult(IClipboardEntry clipboardEntry) : base(clipboardEntry.Title)
        {
            _clipboardEntry = clipboardEntry;
            Subtitle = $"Copied {MakeReadableTimestamp()}";
        }

        private string MakeReadableTimestamp()
        {
            var today = DateTime.Today;
            var actual = _clipboardEntry.Copied;
            if (today.DayOfYear == actual.DayOfYear)
            {
                return $"today on {actual:HH:mm}";
            }

            return actual.ToString("ddd, dd MMM yyy HH':'mm':'ss");
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

            ClipboardHelper.History.AddFirst(_clipboardEntry);
            ClipboardHelper.PasteClipboard();
        }

        public override Bitmap Icon => _clipboardEntry.Icon;
    }
}
