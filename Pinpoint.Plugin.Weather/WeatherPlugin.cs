﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private const string Description = "Look up weather forecasts.\n\nExamples: \"weather <location>\"";
        private readonly Dictionary<string, List<WeatherDayModel>> _weatherCache = new Dictionary<string, List<WeatherDayModel>>();

        public PluginMeta Meta { get; set; } = new PluginMeta("Weather", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool TryLoad() => true;

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return query.Parts.Length == 2 && query.Parts[0].ToLower().Equals("weather");
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var location = query.Parts[1];

            if (_weatherCache.ContainsKey(location))
            {
                foreach (var weatherDayModel in _weatherCache[location])
                {
                    yield return new WeatherResult(weatherDayModel);
                }

                yield break;
            }

            var weather = await LookupWeather(location);
            if (weather == null)
            {
                yield break;
            }

            _weatherCache[location] = weather;

            foreach(var weatherDayModel in weather)
            {
                yield return new WeatherResult(weatherDayModel);
            }
        }

        private async Task<List<WeatherDayModel>> LookupWeather(string location)
        {
            var url = $"http://getpinpoint.herokuapp.com/api/weather/{location}";
            var httpResponse = await SendGet(url);
            if (string.IsNullOrEmpty(httpResponse))
            {
                return null;
            }

            if (httpResponse.Contains("error"))
            {
                return null;
            }

            var days = new List<WeatherDayModel>();
            var result = JObject.Parse(httpResponse)["forecast"]["forecastday"].ToArray();
            foreach (var token in result)
            {
                var weatherDayModel = JsonConvert.DeserializeObject<WeatherDayModel>(token["day"].ToString());
                weatherDayModel.DayOfWeek = DateTime.Parse(token["date"].ToString()).ToString("ddd").Substring(0, 2);
                days.Add(weatherDayModel);
            }

            return days;
        }


        private async Task<string> SendGet(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                return await httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
