using Newtonsoft.Json;

namespace Pinpoint.Plugin.Weather.Models
{
    public class ConditionModel
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon")]
        public string IconUrl { get; set; }
    }
}
