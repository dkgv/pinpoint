using System.Drawing;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Weather.Models;

namespace Pinpoint.Plugin.Weather;

public class WeatherResult : AbstractQueryResult
{
    public WeatherResult(WeatherDayModel m, bool isUS) : base(
        BuildDayResultString(m, isUS),
        BuildDayResultDetailsString(m, isUS))
    {
        Icon = WeatherIconRepository.Get(m.Condition.IconUrl);

        foreach (var weatherHourModel in m.Hours)
        {
            Options.Add(new WeatherResult(weatherHourModel, isUS));
        }
    }

    public WeatherResult(WeatherHourModel m, bool isUS) : base(
        BuildHourResultString(m, isUS),
        BuildHourResultDetailsString(m, isUS))
    {
        Icon = WeatherIconRepository.Get(m.Condition.IconUrl);
    }

    private static string BuildDayResultString(WeatherDayModel m, bool isUS)
    {
        var temperatureUnit = !isUS ? "C" : "F";
        var minTemp = !isUS ? m.MinTempC : m.MinTempF;
        var maxTemp = !isUS ? m.MaxTempC : m.MaxTempF;
        var title = $"{m.DayOfWeek}: {m.Condition.Text} with {minTemp}-{maxTemp} {temperatureUnit}";
        return title;
    }

    private static string BuildDayResultDetailsString(WeatherDayModel m, bool isUS)
    {
        var details = "";
        if (m.ChanceOfRain > 0)
        {
            details += $"Rain {m.ChanceOfRain}% ";
        }

        if (m.ChanceOfSnow > 0)
        {
            if (details.Length > 0)
            {
                details += "| ";
            }
            details += "Snow {m.ChanceOfSnow}% ";
        }

        if (m.PrecipitationMm > 0)
        {
            var precipitationUnit = isUS ? "in" : "mm";
            var precipitation = isUS ? m.PrecipitationIn : m.PrecipitationMm;
            details += $"| Precipitation: {precipitation}{precipitationUnit} ";
        }

        details += $"| Max wind: {m.WindMps}m/s";

        return details;
    }

    private static string BuildHourResultString(WeatherHourModel m, bool isUS)
    {
        var temperatureUnit = !isUS ? "C" : "F";
        var temp = !isUS ? m.TempC : m.TempF;
        var feelsLike = !isUS ? m.FeelsLikeC : m.FeelsLikeF;
        // Extract hour of time (last 5 letters)
        var last5Letters = m.Time.Substring(m.Time.Length - 5);
        var title = $"{last5Letters}: {m.Condition.Text} | {temp} {temperatureUnit} (feels like {feelsLike} {temperatureUnit})";
        return title;
    }

    private static string BuildHourResultDetailsString(WeatherHourModel m, bool isUS)
    {
        var details = "";
        if (m.ChanceOfRain > 0)
        {
            details += $"Rain {m.ChanceOfRain}% ";
        }

        if (m.ChanceOfSnow > 0)
        {
            if (details.Length > 0)
            {
                details += "| ";
            }
            details += $"Snow {m.ChanceOfSnow}% ";
        }

        if (m.PrecipitationMm > 0)
        {
            var precipitationUnit = isUS ? "in" : "mm";
            var precipitation = isUS ? m.PrecipitationIn : m.PrecipitationMm;
            if (details.Length > 0)
            {
                details += "| ";
            }
            details += $"Precipitation: {precipitation}{precipitationUnit} ";
        }

        details += $"Wind {m.WindMps} (gust {m.GustMps}) m/s";
        return details;
    }

    public override Bitmap Icon { get; }

    public override void OnSelect()
    {
    }
}
