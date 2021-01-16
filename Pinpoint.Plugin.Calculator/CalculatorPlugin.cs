using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pinpoint.Core;

namespace Pinpoint.Plugin.Calculator
{
    public class CalculatorPlugin : IPlugin
    {
        public PluginMeta Meta { get; set; } = new PluginMeta("Calculator", PluginPriority.Highest);

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

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var table = new System.Data.DataTable();
            var failed = false;
            var result = 0.0;
            try
            {
                result = Convert.ToDouble(table.Compute(query.RawQuery, string.Empty));
                result = Math.Round(result, 5);
            }
            catch (SyntaxErrorException)
            {
                failed = true;
            }

            if (!failed)
            {
                yield return new CalculatorResult(result.ToString());
            }
        }
    }
}
