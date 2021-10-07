using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyModel
    {
        public CurrencyModel(string @base)
        {
            Base = @base;
        }

        [JsonProperty("rates")] 
        public Dictionary<string, dynamic> Rates { get; set; } = new();

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }
    }
}
