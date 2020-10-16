using System;
using System.Globalization;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyResult : IQueryResult
    {
        public CurrencyResult(double value, string to)
        {
            Title = "= " + value.ToString(CultureInfo.InvariantCulture) + " " + to.ToUpper();
        }

        public string Title { get; }

        public string Subtitle { get; }

        public object Instance { get; }

        public void OnSelect()
        {
        }
    }
}
