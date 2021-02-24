using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.EncodeDecode
{
    public class EncodeDecodeResult: CopyabableQueryOption
    {
        public EncodeDecodeResult(string result): base(result)
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Hashtag;
    }
}