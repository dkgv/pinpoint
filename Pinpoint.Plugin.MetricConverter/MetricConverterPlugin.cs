using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.MetricConverter.Converters;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.MetricConverter
{
    public class MetricConverterPlugin : IPlugin
    {
        //Example of match: 100cm to m | 100cm
        private const string Pattern = @"^(\d+) ?(mm|cm|km|micrometer|nm|mi|yd|ft|in|m){1}( (to|in) )?(mm|cm|km|m|micrometer|nm|mi|yd|ft|in)?";
        
        public PluginMeta Meta { get; set; } = new PluginMeta("Metric Converter", PluginPriority.Highest);

        public bool TryLoad() => true;

        public void Unload()
        {
        }

        private static Tuple<string, double> ConvertQuery(Query query)
        {
            var match = Regex.Match(query.RawQuery, Pattern);
            if (!match.Success)
            {
                return default;
            }

            // match.Groups[0].Value holds the entire matched expression.
            var value = match.Groups[1].Value;
            var fromUnit = match.Groups[2].Value;
            var toUnit = match.Groups[5].Value;
                
            // Handle non specified toUnit.
            if (match.Groups[3].Value == "" || toUnit == "")
            {
                toUnit = "m";
            }

            var conversion = UnitConverter.ConvertTo(fromUnit, toUnit, Convert.ToDouble(value));
            return new Tuple<string, double>(toUnit, Math.Round(conversion, 5));
        }

        public async Task<bool> Activate(Query query)
        {
            return Regex.IsMatch(query.RawQuery, Pattern);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var tuple = ConvertQuery(query);
            yield return new ConversionResult(tuple.Item1, tuple.Item2);
        }
    }
}