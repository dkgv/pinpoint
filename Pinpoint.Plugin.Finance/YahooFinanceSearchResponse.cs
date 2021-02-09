using Newtonsoft.Json;

namespace Pinpoint.Plugin.Finance
{
    public class YahooFinanceSearchResponse
    {
        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("longName")]
        public string LongName { get; set; }
    }
}