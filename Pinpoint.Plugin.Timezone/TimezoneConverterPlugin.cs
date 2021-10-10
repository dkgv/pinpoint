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
        private const string Description = "Convert time to/from different timezones.\n\nExamples: \"time in PST\", \"12:01 to EST\", \"12:00 am GMT to PST\", \"13:24 CET to JST\"";

        private const string TwelveHourTime = @"(1[0-2]|0?[1-9]):[0-5][0-9] (AM|am|PM|pm)";
        private const string TwentyFourHourTime = @"([01]\d|2[0-3]):([0-5]\d)";

        private static readonly Dictionary<string, string> TimezoneIdConversion = new()
        {
            { "GMT", "Greenwich Standard Time" },
            { "PST", "Pacific Standard Time" },
            { "EST", "Eastern Standard Time" },
            { "CEST", "Central European Standard Time" },
            { "CET", "Central European Standard Time" },
            { "JST", "Tokyo Standard Time" },
        };

        private static readonly string Timezone = string.Join("|", TimezoneIdConversion.Keys);

        private static readonly Regex Pattern =
            new($"^(time|({TwentyFourHourTime}) ?({Timezone})?|({TwelveHourTime}) ?({Timezone})?) (to|in) ({Timezone})$");

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
                var time = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(targetTimezone));

                yield return new TimezoneConversionResult(time.ToShortTimeString());
            }
            else
            {
                // Bit of a mess - handling all the different types of input strings.
                var queryTime = splitQuery[0];
                var sourceTimezone = ConvertTimezoneCode(splitQuery[1]);
                var targetTimezone = ConvertTimezoneCode(splitQuery[^1]);
                
                var isOnTwelveHourForm = 
                    splitQuery[1] == "am" || splitQuery[1] == "pm" ||
                    splitQuery[1] == "AM" || splitQuery[1] == "PM";
                
                if (isOnTwelveHourForm)
                {
                    queryTime = $"{splitQuery[0]} {splitQuery[1]}";
                    sourceTimezone = ConvertTimezoneCode(splitQuery[2]);
                }

                var time = TimeZoneInfo.ConvertTime(DateTime.Parse(queryTime),
                    TimeZoneInfo.FindSystemTimeZoneById(sourceTimezone), 
                    TimeZoneInfo.FindSystemTimeZoneById(targetTimezone));
                
                var formattedTime = isOnTwelveHourForm ? time.ToString("h:mm tt") : time.ToShortTimeString();
                yield return new TimezoneConversionResult(formattedTime);
            }
        }

        private static string ConvertTimezoneCode(string code)
        {
            return TimezoneIdConversion.TryGetValue(code, out code) ? code : TimeZoneInfo.Local.Id;
        }
    }
}