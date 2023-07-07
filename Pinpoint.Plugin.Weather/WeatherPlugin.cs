using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Weather.Models;

namespace Pinpoint.Plugin.Weather
{
    public class WeatherPlugin : AbstractPlugin
    {
        private const string KeyDefaultCity = "Default city";
        private readonly Dictionary<string, List<WeatherDayModel>> _weatherCache = new();

        public override PluginManifest Manifest { get; } = new("Weather", PluginPriority.High)
        {
            Description = "Look up weather forecasts.\n\nExamples: \"weather <location>\" or \"weather\" if default location is set"
        };

        public override PluginState State => new()
        {
            DebounceTime = TimeSpan.FromMilliseconds(200)
        };

        public override PluginStorage Storage => new()
        {
            User = new UserSettings{
                {KeyDefaultCity, string.Empty}
            }
        };

        public override async Task<bool> ShouldActivate(Query query)
        {
            var hasWeatherPrefix = query.Parts[0].ToLower().Equals("weather");
            if (!hasWeatherPrefix)
            {
                return false;
            }

            return !string.IsNullOrEmpty(GetDefaultCity()) || query.Parts.Length >= 2;
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var location = GetLocation(query);
            var isUS = IsUS();

            if (_weatherCache.TryGetValue(location, out var weather))
            {
                foreach (var weatherDayModel in weather)
                {
                    yield return new WeatherResult(weatherDayModel, isUS);
                }
            }
            else
            {
                weather = await GetWeatherAt(location);
                if (weather == null)
                {
                    yield break;
                }

                _weatherCache[location] = weather;

                foreach (var weatherDayModel in weather)
                {
                    yield return new WeatherResult(weatherDayModel, isUS);
                }
            }
        }

        private bool IsUS()
        {
            var currentRegion = new RegionInfo(CultureInfo.CurrentCulture.LCID);
            return !currentRegion.IsMetric;
        }

        private string GetLocation(Query query)
        {
            if (query.Parts.Length == 1)
            {
                return GetDefaultCity();
            }

            return string.Join(" ", query.Parts[1..]).Trim();
        }

        private string GetDefaultCity()
        {
            return Storage.User.Str(KeyDefaultCity);
        }

        private async Task<List<WeatherDayModel>> GetWeatherAt(string location)
        {
            var url = $"https://usepinpoint.com/api/weather/{location.ToLower()}";

            var result = await HttpHelper.SendGet(url, response =>
            {
                if (response.Contains("error"))
                {
                    return null;
                }

                return JObject.Parse(response)["forecast"]["forecastday"];
            });

            if (result == null)
            {
                return new();
            }

            return result.Select(token =>
            {
                var dayModel = JsonConvert.DeserializeObject<WeatherDayModel>(token["day"].ToString());
                dayModel.DayOfWeek = DateTime.Parse(token["date"].ToString()).ToString("ddd").Substring(0, 2);
                dayModel.Hours = token["hour"]
                    .Select(t => JsonConvert.DeserializeObject<WeatherHourModel>(t.ToString()))
                    .ToArray();

                return dayModel;
            }).ToList();
        }
    }
}
