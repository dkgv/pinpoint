using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.MetricConverter.Converters;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.MetricConverter
{
    public class MetricConverterPlugin : IPlugin
    {
        //Example of match: 100cm to m | 100cm
        private static readonly Regex Pattern = new Regex(@"^(\d+) ?(mm|cm|km|micrometer|nm|mi|yd|ft|in|m){1}( (to|in) )?(mm|cm|km|m|micrometer|nm|mi|yd|ft|in)?");
        private static Match _match;
        private const string Description = "Convert between metric/imperial units.\n\nExamples: \"100 cm to m\", \"10in to mm\"";

        public PluginMeta Meta { get; set; } = new PluginMeta("Metric Converter", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public void Unload()
        {
        }

        private static Tuple<string, double> ConvertQuery(Query query)
        {
            // match.Groups[0].Value holds the entire matched expression.
            var value = _match.Groups[1].Value;
            var fromUnit = _match.Groups[2].Value;
            var toUnit = _match.Groups[5].Value;
                
            // Handle non specified toUnit.
            if (_match.Groups[3].Value == "" || toUnit == "")
            {
                toUnit = "m";
            }

            var conversion = UnitConverter.ConvertTo(fromUnit, toUnit, Convert.ToDouble(value));
            return new Tuple<string, double>(toUnit, Math.Round(conversion, 5));
        }

        public async Task<bool> Activate(Query query)
        {
            _match = Pattern.Match(query.RawQuery);
            return _match != default && _match.Success;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var tuple = ConvertQuery(query);
            yield return new ConversionResult(tuple.Item1, tuple.Item2);
        }
    }
}