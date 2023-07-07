using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Finance
{
    public class FinancePlugin : IPlugin
    {
        private readonly YahooFinanceApi _yahooFinanceApi = new(TimeSpan.FromMinutes(2));

        public PluginManifest Manifest { get; set; } = new("Finance Plugin", PluginPriority.High)
        {
            Description = "Look up stock tickers.\n\nExamples: \"$GME\", \"$MSFT\""
        };

        public PluginStorage Storage { get; set; } = new();

        public TimeSpan DebounceTime => TimeSpan.FromMilliseconds(200);

        public async Task<bool> Activate(Query query)
        {
            if (query.Parts.Length != 1 || !query.Prefix().Equals("$") || query.RawQuery.Length < 3)
            {
                return false;
            }

            var ticker = query.Parts[0].Substring(1);
            return ticker.All(ch => char.IsLetter(ch) || ch == '.' || ch == '^') && ticker.Count(char.IsLetter) >= 2;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            // $GME => GME
            var ticker = query.Parts[0][1..];
            var priceResponseTask = _yahooFinanceApi.LookupPrice(ticker);
            var searchResponseTask = _yahooFinanceApi.Search(ticker);

            await Task.WhenAll(priceResponseTask, searchResponseTask);

            if (priceResponseTask.Result == null || searchResponseTask.Result == null)
            {
                yield break;
            }

            yield return new FinanceSearchResult(priceResponseTask.Result, searchResponseTask.Result);
        }
    }
}