using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Calculator
{
    public class CalculatorResult : CopyabableQueryOption
    {
        public CalculatorResult(string result) : base("= " + result, result)
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Calculator;
    }
}
