using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Calculator
{
    public class CalculatorPlugin : AbstractPlugin
    {
        private static readonly Regex MathPattern = new(@"([-+]?[0-9]*\.?[0-9]+[\/\+\-\*])+([-+]?[0-9]*\.?[0-9]+)");

        public override PluginManifest Manifest { get; } = new("Calculator", PluginPriority.High)
        {
            Description = "Evaluate mathematical expressions quickly.\n\nExamples: \"9+5*(123/5)\""
        };

        public override async Task<bool> ShouldActivate(Query query)
        {
            var trimmedQuery = query.Raw.Replace(" ", "");
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

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var table = new DataTable();
            var failed = true;
            var result = 0.0;
            try
            {
                result = Convert.ToDouble(table.Compute(query.Raw, string.Empty));
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
