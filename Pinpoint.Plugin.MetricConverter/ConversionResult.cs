using FontAwesome5;

namespace Pinpoint.Plugin.MetricConverter
{
    public class ConversionResult : AbstractFontAwesomeQueryResult
    {
        public ConversionResult(string result) : base("= " + result)
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Ruler;

        public override void OnSelect()
        {
        }
    }
}