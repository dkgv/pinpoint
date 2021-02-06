using Newtonsoft.Json;

namespace Pinpoint.Plugin.Finance
{
    public class YahooFinancePriceResponse
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }
        
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        
        [JsonProperty("exchangeName")]
        public string ExchangeName { get; set; }
        
        [JsonProperty("instrumentType")]
        public string InstrumentType { get; set; }
        
        [JsonProperty("regularMarketPrice")]
        public float RegularMarketPrice { get; set; }
        
        [JsonProperty("previousClose")]
        public float PreviousClose { get; set; }

        public string Url => "https://finance.yahoo.com/quote/" + Symbol;
    }
}