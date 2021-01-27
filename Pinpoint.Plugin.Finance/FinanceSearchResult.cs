using System;
using FontAwesome5;
using Pinpoint.Core;

namespace Pinpoint.Plugin.Finance
{
    public class FinanceSearchResult : UrlQueryResult
    {
        public FinanceSearchResult(YahooFinanceResponse response) : base(response.Url)
        {
            var pricePrefix = response.RegularMarketPrice > response.PreviousClose
                ? "+" 
                : "-";
            var priceChange = Math.Abs(PriceChange(response));
            var percentChange = Math.Abs(PercentChange(response));

            Title = response.Symbol + " @ " + response.RegularMarketPrice + " " + pricePrefix + priceChange + " (" + pricePrefix + percentChange + "%)";
        }

        private double PriceChange(YahooFinanceResponse response)
        {
            return Math.Round(response.RegularMarketPrice - response.PreviousClose, 2);
        }

        private double PercentChange(YahooFinanceResponse response)
        {
            return Math.Round(Math.Abs(response.PreviousClose - response.RegularMarketPrice) /
                ((response.PreviousClose + response.RegularMarketPrice) / 2) * 100f, 2);
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_ChartLine;
    }
}