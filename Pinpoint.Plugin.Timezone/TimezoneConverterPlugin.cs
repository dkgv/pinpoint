using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Timezone
{
    public class TimezoneConverterPlugin : AbstractPlugin
    {
        public override PluginManifest Manifest { get; } = new("Timezone Converter", PluginPriority.High)
        {
            Description = "Convert time to/from different timezones.\n\nExamples: \"time in PST\", \"12:01 to EST\", \"12:00 am GMT to PST\", \"13:24 CET to JST\", \"12am GMT+1 to PST\", \"12 am EST +2 to GMT\", \"16 GMT to EST\", \"16:00 GMT to EST\""
        };

        private static readonly Regex TimeInZonePattern = new(@"^time in (?<timezone>\w+)(?<offset>[+-]\d+)?$", RegexOptions.IgnoreCase);
        private static readonly Regex ConvertTimePattern = new(@"^(?<time>\d{1,2}(:\d{2})?(\s?(am|pm)?)?)\s?(?<sourceZone>\w+)?\s?(?<sourceOffset>[+-]\d+)?\s?to\s?(?<targetZone>\w+)(?<targetOffset>[+-]\d+)?$", RegexOptions.IgnoreCase);

        public override async Task<bool> ShouldActivate(Query query)
        {
            return TimeInZonePattern.IsMatch(query.Raw) || ConvertTimePattern.IsMatch(query.Raw);
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            string result = null;

            // Match query against the "Time in Zone" pattern
            var match = TimeInZonePattern.Match(query.Raw);
            if (match.Success)
            {
                var targetZone = match.Groups["timezone"].Value.ToUpperInvariant();
                int targetOffset = ParseOffset(match.Groups["offset"].Value);

                try
                {
                    result = GetCurrentTimeInZone(targetZone, targetOffset);
                }
                catch (TimeZoneNotFoundException)
                {
                    result = $"Timezone '{targetZone}' not recognized.";
                }

                if (result != null)
                {
                    yield return new TimezoneConversionResult(result);
                }

                yield break;
            }

            // Match query against the "Convert Time" pattern
            match = ConvertTimePattern.Match(query.Raw);
            if (match.Success)
            {
                var timeInput = match.Groups["time"].Value;
                var sourceZone = match.Groups["sourceZone"].Success ? match.Groups["sourceZone"].Value.ToUpperInvariant() : "UTC";
                var sourceOffset = ParseOffset(match.Groups["sourceOffset"].Value);
                var targetZone = match.Groups["targetZone"].Value.ToUpperInvariant();
                var targetOffset = ParseOffset(match.Groups["targetOffset"].Value);

                try
                {
                    result = ConvertTimeBetweenZones(timeInput, sourceZone, sourceOffset, targetZone, targetOffset);
                }
                catch (Exception ex) when (ex is TimeZoneNotFoundException || ex is FormatException)
                {
                    result = $"Invalid input or timezone not recognized: {ex.Message}";
                }

                if (result != null)
                {
                    yield return new TimezoneConversionResult(result);
                }

                yield break;
            }
        }

        private int ParseOffset(string offset)
        {
            if (string.IsNullOrEmpty(offset))
            {
                return 0;
            }

            return int.TryParse(offset, out int parsedOffset) ? parsedOffset : 0;
        }

        private string GetCurrentTimeInZone(string timezoneId, int offset)
        {
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            var currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timezone);

            // Apply offset if provided
            currentTime = currentTime.AddHours(offset);

            return $"{currentTime.ToString("HH:mm")} {timezoneId}";
        }

        private string ConvertTimeBetweenZones(string timeInput, string sourceZoneId, int sourceOffset, string targetZoneId, int targetOffset)
        {
            var sourceZone = TimeZoneInfo.FindSystemTimeZoneById(sourceZoneId);
            var targetZone = TimeZoneInfo.FindSystemTimeZoneById(targetZoneId);

            // Handle both "12", "12am", "12 am", "12:00 am", "16", "16:00"
            if (!DateTime.TryParseExact(timeInput, new[] { "h:mm tt", "H:mm", "h tt", "htt", "HH", "HH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
            {
                throw new FormatException($"Could not parse time '{timeInput}'.");
            }

            // Apply source offset before conversion
            parsedTime = parsedTime.AddHours(-sourceOffset);

            var sourceTime = TimeZoneInfo.ConvertTimeToUtc(parsedTime, sourceZone);
            var targetTime = TimeZoneInfo.ConvertTimeFromUtc(sourceTime, targetZone);

            // Apply target offset after conversion
            targetTime = targetTime.AddHours(targetOffset);

            return $"{targetTime.ToString("HH:mm")} {targetZoneId}{(targetOffset != 0 ? $" (Offset: {targetOffset:+#;-#;+0})" : "")}";
        }
    }
}
