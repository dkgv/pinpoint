using System.Drawing;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Weather.Models;

namespace Pinpoint.Plugin.Weather
{
    public class WeatherResult : AbstractQueryResult
    {
        public WeatherResult(WeatherDayModel m) : base(
            m.DayOfWeek + ": " + m.Condition.Text + " | " + m.MinTempC + "-" + m.MaxTempC + " C | " + m.MinTempF + "-" + m.MaxTempF + " F",
            "Rain " + m.ChanceOfRain + "% | Snow " + m.ChanceOfSnow + "% " 
                + (m.PrecipitationMm > 0 ? "| Precipitation: " + m.PrecipitationMm + "mm/" + m.PrecipitationIn + "in " : "| ")
            + "| Max wind: " + m.WindMps + "m/s"
            )
        {
            Icon = WeatherIconRepository.Get(m.Condition.IconUrl);

            foreach (var weatherHourModel in m.Hours)
            {
                Options.Add(new WeatherResult(weatherHourModel));
            }
        }

        public WeatherResult(WeatherHourModel m) : base(
            m.Time + ": " + m.Condition.Text + " | " + m.TempC + " (feels " + m.FeelsLikeC + ") C | " + m.TempF + " (feels " + m.FeelsLikeF + ") F",
            "Rain " + m.ChanceOfRain + "% | Snow " + m.ChanceOfSnow + "% " 
                + (m.PrecipitationMm > 0 ? "| Precipitation: " + m.PrecipitationMm + "mm/" + m.PrecipitationIn + "in | " : "| ") 
            + "Wind " + m.WindMps + " (gust " + m.GustMps + ") m/s"
            )
        {
            Icon = WeatherIconRepository.Get(m.Condition.IconUrl);
        }

        public override Bitmap Icon { get; }

        public override void OnSelect()
        {
        }
    }
}
