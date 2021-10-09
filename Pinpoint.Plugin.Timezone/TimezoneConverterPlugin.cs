using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Timezone
{
    public class TimezoneConverterPlugin : IPlugin
    {
        private const string Description = "Convert time to/from different timezones.\n\nExamples: \"time in PST\", \"12:00 am GMT to PST\", \"13:24 CET to JST\"";

        private const string TwelveHourTime = @"(1[0-2]|0?[1-9]):?[0-5][0-9] (AM|am|PM|pm)";
        private const string TwentyFourHourTime = @"([01]\d|2[0-3]):?([0-5]\d)";
        private const string Timezone = "GMT|PST|EST|CEST|CET|JST";

        private static readonly Regex Pattern =
            new($"((time)|({TwelveHourTime} ({Timezone}))|({TwentyFourHourTime} ({Timezone}))) (to|in) ({Timezone})");
        public PluginMeta Meta { get; set; } = new("Timezone Converter", Description, PluginPriority.Highest);
        public PluginSettings UserSettings { get; set; } = new();
        public async Task<bool> Activate(Query query)
        {
             return Pattern.IsMatch(query.RawQuery);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var splitQuery = query.RawQuery.Split(" ");
            if (query.RawQuery.Contains("time"))
            {
                var targetTimezone = ConvertTimezoneCode(splitQuery[2]);
                var url = $"https://www.timeapi.io/api/Time/current/zone?timeZone={targetTimezone}";

                var time = await HttpHelper.SendGet(url, JObject.Parse);
                yield return new TimezoneConversionResult(time["time"].ToString());
            }
            else
            {
                var queryTime = $"{splitQuery[0]}:00";
                var sourceTimezone = ConvertTimezoneCode(splitQuery[1]);
                var targetTimezone = ConvertTimezoneCode(splitQuery[3]);
                
                var isOnTwelveHourForm = 
                    splitQuery[1] == "am" || splitQuery[1] == "pm" ||
                    splitQuery[1] == "AM" || splitQuery[1] == "PM";

                if (isOnTwelveHourForm)
                {
                    queryTime = ConvertToMilitaryTime($"{splitQuery[0]} {splitQuery[1]}");
                    sourceTimezone = ConvertTimezoneCode(splitQuery[2]);
                    targetTimezone = ConvertTimezoneCode(splitQuery[4]);
                }
                
                const string url = "https://www.timeapi.io/api/Conversion/ConvertTimeZone";
                var jsonQuery = new
                {
                    fromTimezone = sourceTimezone,
                    dateTime = $"{DateTime.Today:yyyy-MM-dd} {queryTime}",
                    toTimezone = targetTimezone,
                    dstAmbiguity = ""
                };
                
                var time = await HttpHelper.SendPost(url, JsonContent.Create(jsonQuery), 
                     message => JObject.Parse(message.Content.ReadAsStringAsync(ct).Result));
                
                var returnTime = time["conversionResult"]["time"].ToString();
                var formattedTime = isOnTwelveHourForm ? DateTime.Parse(returnTime).ToString("h:mm tt") : returnTime;

                yield return new TimezoneConversionResult(formattedTime);
            }
        }

        private static string ConvertTimezoneCode(string code)
        {
            return code switch // TODO: Add more (all) timezones.
            {
                "GMT" => "Etc/Greenwich",
                "PST" => "America/Los_Angeles",
                "EST" => "America/New_York",
                "CEST" => "Europe/Amsterdam",
                "CET" => "Europe/Amsterdam",
                "JST" => "Asia/Tokyo",
                _ => "Europe/Amsterdam"
            };
        }

        private static string ConvertToMilitaryTime(string time)
        {
            return DateTime.Parse(time).ToString("HH:mm:ss");
        }
    }
}