using FontAwesome5;

namespace Pinpoint.Plugin.MetricConverter
{
    public class ConversionResult : AbstractQueryResult
    {
        public ConversionResult(string result) : base("= " + result)
        {
        }

        public override EFontAwesomeIcon Icon => EFontAwesomeIcon.Solid_Ruler;

        public override void OnSelect()
        {
        }
    }
}