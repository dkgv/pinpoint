using System;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.Weather.Models
{
    public class WeatherDayModel
    {
        public WeatherHourModel[] Hours { get; set; }

        public string DayOfWeek { get; set; }

        [JsonProperty("maxtemp_c")]
        public float MaxTempC { get; set; }

        [JsonProperty("mintemp_c")]
        public float MinTempC { get; set; }

        [JsonProperty("maxtemp_f")]
        public float MaxTempF { get; set; }

        [JsonProperty("maxwind_mph")]
        public float WindMph { get; set; }

        [JsonProperty("maxwind_kph")]
        public float WindKph { get; set; }

        public float WindMps => (float) Math.Round(WindKph * 0.27777777777846f, 2);

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
