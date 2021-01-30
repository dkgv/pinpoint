using FontAwesome5;
using Pinpoint.Core.Results;

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
