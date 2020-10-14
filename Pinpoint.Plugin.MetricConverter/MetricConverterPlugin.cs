using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pinpoint.Plugin.MetricConverter;
using Pinpoint.Plugin.MetricConverter.Converters;

namespace Pinpoint.Plugin.MetricConverter
{
    public class MetricConverterPlugin : IPlugin
    {
        //Example of match: 100cm to m | 100cm
        private const string Pattern = @"^(\d+)(mm|cm|km|m|micrometer|nm|mi|yd|ft|in){1}( to )?(mm|cm|km|m|micrometer|nm|mi|yd|ft|in)?$";
        
        public PluginMeta Meta { get; set; } = new PluginMeta("MetricConverter", true);
        public void Load()
        {
        }

        public void Unload()
        {
        }

        private static string ConvertQuery(Query query)
        {
            var match = Regex.Match(query.RawQuery, Pattern);
            if (!match.Success) return "";

            //match.Groups[0].Value holds the entire matched expression.
            var value = match.Groups[1].Value;
            var fromUnit = match.Groups[2].Value;
            var toUnit = match.Groups[4].Value;
                
            //Handle non specified toUnit.
            if (match.Groups[3].Value == "" || toUnit == "")
            {
                toUnit = "m";
            }

            var returnUnitConverter = new UnitConverter(Convert.ToDouble(value), fromUnit).ConvertTo(toUnit);
            returnUnitConverter.Amount = Math.Round(returnUnitConverter.Amount, 3);

            return returnUnitConverter.ToString();
        }

        public async Task<bool> Activate(Query query)
        {
            return Regex.IsMatch(query.RawQuery, Pattern);
        }

        public async IAsyncEnumerable<IQueryResult> Process(Query query)
        {
            yield return new ConversionResult(query.RawQuery, ConvertQuery(query));
        }
    }
}