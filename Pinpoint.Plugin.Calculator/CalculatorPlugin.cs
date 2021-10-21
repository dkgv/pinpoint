using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Calculator
{
    public class CalculatorPlugin : IPlugin
    {
        private const string Description = "Evaluate mathematical expressions quickly.\n\nExamples: \"9+5*(123/5)\"";
        private static readonly Regex MathPattern = new(@"([-+]?[0-9]*\.?[0-9]+[\/\+\-\*])+([-+]?[0-9]*\.?[0-9]+)");
        
        public PluginMeta Meta { get; set; } = new("Calculator", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new();

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            var trimmedQuery = query.RawQuery.Replace(" ", "");
            // Ensure query ends with 0-9 or )
            var lastChar = trimmedQuery[^1];
            if (!char.IsDigit(lastChar) && lastChar != ')')
            {
                return false;
            }


            // Ensure query has same num open as close brackets
            var opens = trimmedQuery.Count(c => c == '(');
            var closes = trimmedQuery.Count(c => c == ')');
            if (opens != closes)
            {
                return false;
            }

            return MathPattern.IsMatch(trimmedQuery);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
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
