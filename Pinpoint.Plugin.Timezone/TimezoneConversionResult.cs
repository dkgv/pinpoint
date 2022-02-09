using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Timezone
{
    public class TimezoneConversionResult : CopyabableQueryOption
    {
        public TimezoneConversionResult(string title, string content) : base(title, content)
        {
        }

        public TimezoneConversionResult(string content) : base(content)
        {
        }
        
        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Clock;
    }
}