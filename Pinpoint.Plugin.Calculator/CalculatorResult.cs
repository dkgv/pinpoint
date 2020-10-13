using System;

namespace Pinpoint.Plugin.Calculator
{
    public class CalculatorResult : IQueryResult
    {
        public CalculatorResult(string expression, string result)
        {
            Title = "= " + result;
        }

        public string Title { get; }

        public string Subtitle { get; }

        public object Instance { get; }

        public void OnSelect()
        {
        }
    }
}
