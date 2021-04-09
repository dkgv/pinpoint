using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.PasswordGenerator
{
    public class PasswordResult: CopyabableQueryOption
    {
        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Key;

        public PasswordResult(string content) : base(content)
        {
        }

        public override void OnSelect()
        {
            base.OnSelect();

            ClipboardHelper.Copy(Content);
            ClipboardHelper.PasteClipboard();
        }
    }
}