using Newtonsoft.Json;

namespace Pinpoint.Plugin.Weather.Models
{
    public class WeatherDayModel
    {
        public string DayOfWeek { get; set; }

        [JsonProperty("maxtemp_c")]
        public float MaxTempC { get; set; }

        [JsonProperty("mintemp_c")]
        public float MinTempC { get; set; }

        [JsonProperty("maxtemp_f")]
        public float MaxTempF { get; set; }

        [JsonProperty("mintemp_f")]
        public float MinTempF { get; set; }

        [JsonProperty("totalprecip_mm")]
        public float PrecipitationMm { get; set; }

        [JsonProperty("totalprecip_in")]
        public float PrecipitationIn { get; set; }

        [JsonProperty("daily_chance_of_rain")]
        public float ChanceOfRain { get; set; }

        [JsonProperty("daily_chance_of_snow")]
        public float ChanceOfSnow { get; set; }

        [JsonProperty("condition")]
        public ConditionModel Condition { get; set; }
    }
}
