using FontAwesome5;
using Pinpoint.Core;

namespace Pinpoint.Plugin.EncodeDecode
{
    public class EncodeDecodeResult: AbstractFontAwesomeQueryResult
    {
        public EncodeDecodeResult(string result): base(result)
        {
            
        }
        public override void OnSelect()
        {
            
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Hashtag;
    }
}