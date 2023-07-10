using System.Diagnostics;
using FontAwesome5;
using Pinpoint.Core.Clipboard;

namespace Pinpoint.Core.Results
{
    public class CopyabableQueryOption : AbstractFontAwesomeQueryResult
    {
        public CopyabableQueryOption(string title, string content) : base(title, "Copy content to clipboard")
        {
            Content = content;
        }

        public CopyabableQueryOption(string content) : base(content, "Copy content to clipboard")
        {
            Content = content;
        }

        public string Content { get; set; }

        public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Regular_Copy;

        public override void OnSelect()
        {
            ClipboardHelper.CopyUtf8(Content);
        }
    }
}
