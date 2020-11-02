using FontAwesome5;

namespace Pinpoint.Plugin.MetricConverter
{
    public class ConversionResult : AbstractFontAwesomeQueryResult
    {
        public ConversionResult(string to, double amount) : base("= " + amount + " " + to)
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Ruler;

        public override void OnSelect()
        {
        }
    }
}