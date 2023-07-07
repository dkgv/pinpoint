﻿using System;
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
    public class WeatherPlugin : IPlugin
    {
        private const string KeyDefaultCity = "Default city";
        private readonly Dictionary<string, List<WeatherDayModel>> _weatherCache = new();

        public PluginManifest Manifest { get; set; } = new("Weather", PluginPriority.Highest)
        {
            Description = "Look up weather forecasts.\n\nExamples: \"weather <location>\" or \"weather\" if default location is set"
        };

        public PluginStorage Storage { get; set; } = new();

        public TimeSpan DebounceTime => TimeSpan.FromMilliseconds(200);

        public async Task<bool> TryLoad()
        {
            if (Storage.UserSettings.Count == 0)
            {
                Storage.UserSettings.Put(KeyDefaultCity, string.Empty);
            }

            return true;
        }

        public async Task<bool> Activate(Query query)
        {
            var hasWeatherPrefix = query.Parts[0].ToLower().Equals("weather");
            if (!hasWeatherPrefix)
            {
                return false;
            }

            return !string.IsNullOrEmpty(GetDefaultCity()) || query.Parts.Length >= 2;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
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
            return Storage.UserSettings.Str(KeyDefaultCity);
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
