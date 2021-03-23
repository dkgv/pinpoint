using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Calculator
{
    public class CalculatorPlugin : IPlugin
    {
        private static readonly Regex MathPattern = new Regex(@"([-+]?[0-9]*\.?[0-9]+[\/\+\-\*])+([-+]?[0-9]*\.?[0-9]+)");
        
        public PluginMeta Meta { get; set; } = new PluginMeta("Calculator", PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool TryLoad() => true;

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            // Ensure query ends with 0-9 or )
            var lastChar = query.RawQuery[^1];
            if (!char.IsDigit(lastChar) && lastChar != ')')
            {
                return false;
            }

            // Ensure query has same num open as close brackets
            var opens = query.RawQuery.Count(c => c == '(');
            var closes = query.RawQuery.Count(c => c == ')');
            if (opens != closes)
            {
                return false;
            }

            return MathPattern.IsMatch(query.RawQuery);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var table = new DataTable();
            var failed = true;
            var result = 0.0;
            try
            {
                result = Convert.ToDouble(table.Compute(query.RawQuery, string.Empty));
                result = Math.Round(result, 5);
                failed = false;
            }
            catch (SyntaxErrorException)
            {
            }
            catch (OverflowException)
            {
            }

            if (!failed)
            {
                yield return new CalculatorResult(result.ToString());
            }
        }
    }
}
