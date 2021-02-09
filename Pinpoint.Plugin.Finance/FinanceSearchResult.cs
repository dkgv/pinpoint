using System;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Finance
{
    public class FinanceSearchResult : UrlQueryResult
    {
        public FinanceSearchResult(YahooFinancePriceResponse priceResponse, YahooFinanceSearchResponse searchResponse) : base(priceResponse.Url)
        {
            var pricePrefix = priceResponse.RegularMarketPrice > priceResponse.PreviousClose
                ? "+" 
                : "-";
            var priceChange = Math.Abs(PriceChange(priceResponse));
            var percentChange = Math.Abs(PercentChange(priceResponse));

            Title = searchResponse.ShortName + " (" + priceResponse.Symbol + ") @ " + priceResponse.RegularMarketPrice + " " + pricePrefix + priceChange + " (" + pricePrefix + percentChange + "%)";
        }

        private double PriceChange(YahooFinancePriceResponse priceResponse)
        {
            return Math.Round(priceResponse.RegularMarketPrice - priceResponse.PreviousClose, 2);
        }

        private double PercentChange(YahooFinancePriceResponse priceResponse)
        {
            return Math.Round((priceResponse.PreviousClose - priceResponse.RegularMarketPrice) / priceResponse.PreviousClose * 100f, 2);
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_ChartLine;
    }
}