using FontAwesome5;

namespace Pinpoint.Plugin.Calculator
{
    public class CalculatorResult : IQueryResult
    {
        public CalculatorResult(string result)
        {
            Title = "= " + result;
        }

        public string Title { get; }

        public string Subtitle { get; }

        public object Instance { get; }

        public EFontAwesomeIcon Icon => EFontAwesomeIcon.Solid_Calculator;

        public void OnSelect()
        {
        }
    }
}
