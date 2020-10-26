using FontAwesome5;

namespace Pinpoint.Plugin.Calculator
{
    public class CalculatorResult : AbstractQueryResult
    {
        public CalculatorResult(string result) : base("= " + result)
        {
        }

        public override EFontAwesomeIcon Icon => EFontAwesomeIcon.Solid_Calculator;

        public override void OnSelect()
        {
        }
    }
}
