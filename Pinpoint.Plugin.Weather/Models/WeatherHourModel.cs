using System;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.Weather.Models
{
    public class WeatherHourModel
    {
        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("temp_c")]
        public float TempC { get; set; }

        [JsonProperty("temp_f")]
        public float TempF { get; set; }

        [JsonProperty("feelslike_c")]
        public float FeelsLikeC { get; set; }

        [JsonProperty("feelslike_f")]
        public float FeelsLikeF { get; set; }

        [JsonProperty("condition")]
        public ConditionModel Condition { get; set; }

        [JsonProperty("wind_mph")]
        public float WindMph { get; set; }

        [JsonProperty("wind_kph")]
        public float WindKph { get; set; }

        [JsonProperty("gust_mph")]
        public float GustMph { get; set; }

        [JsonProperty("gust_kph")]
        public float GustKph { get; set; }

        [JsonProperty("precip_mm")]
        public float PrecipitationMm { get; set; }

        [JsonProperty("precip_in")]
        public float PrecipitationIn { get; set; }

        [JsonProperty("chance_of_rain")]
        public float ChanceOfRain { get; set; }

        [JsonProperty("chance_of_snow")]
        public float ChanceOfSnow { get; set; }

        public float WindMps => (float)Math.Round(WindKph * 0.27777777777846f, 2);

        public float GustMps => (float)Math.Round(GustKph * 0.27777777777846f, 2);
    }
}
