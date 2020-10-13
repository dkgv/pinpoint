using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Calculator
{
    public class CalculatorPlugin : IPlugin
    {
        public PluginMeta Meta { get; set; } = new PluginMeta("Calculator", true);

        public void Load()
        {
        }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            var lastChar = query.RawQuery[^1];
            if (!char.IsDigit(lastChar) && lastChar != ')')
            {
                return false;
            }

            var opens = query.RawQuery.Count(c => c == '(');
            var closes = query.RawQuery.Count(c => c == ')');
            if (opens != closes)
            {
                return false;
            }

            var pattern = @"([-+]?[0-9]*\.?[0-9]+[\/\+\-\*])+([-+]?[0-9]*\.?[0-9]+)";
            return Regex.IsMatch(query.RawQuery, pattern);
        }

        public async IAsyncEnumerable<IQueryResult> Process(Query query)
        {
            var table = new System.Data.DataTable();
            var result = Convert.ToDouble(table.Compute(query.RawQuery, string.Empty));
            result = Math.Round(result, 5);
            yield return new CalculatorResult(query.RawQuery, result.ToString());
        }
    }
}
