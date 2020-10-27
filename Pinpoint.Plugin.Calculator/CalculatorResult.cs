using FontAwesome5;

namespace Pinpoint.Plugin.Calculator
{
    public class CalculatorResult : AbstractFontAwesomeQueryResult
    {
        public CalculatorResult(string result) : base("= " + result)
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Calculator;

        public override void OnSelect()
        {
        }
    }
}
