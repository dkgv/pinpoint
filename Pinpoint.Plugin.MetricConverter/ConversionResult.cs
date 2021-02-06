using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.MetricConverter
{
    public class ConversionResult : CopyabableQueryOption
    {
        public ConversionResult(string to, double amount) : base("= " + amount + " " + to, "" + amount)
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Ruler;

        public override void OnSelect()
        {
        }
    }
}