using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Finance
{
    public class FinancePlugin : IPlugin
    {
        private readonly YahooFinanceApi _yahooFinanceApi = new YahooFinanceApi(TimeSpan.FromMinutes(2));

        public PluginMeta Meta { get; set; } = new PluginMeta("Finance Plugin", PluginPriority.Highest);
        
        public void Load()
        {
        }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return query.Parts.Length == 1
                   && query.Prefix().Equals("$")
                   && query.RawQuery.Length >= 3
                   && query.Parts[0].Substring(1).All(ch => !char.IsDigit(ch) && (char.IsUpper(ch) || !char.IsLetter(ch)))
                   && query.Parts[0].Substring(1).Length < 5;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            // $GME => GME
            var ticker = query.Parts[0].Substring(1);
            var response = await _yahooFinanceApi.Lookup(ticker);
            if (response != null)
            {
                yield return new FinanceSearchResult(response);
            }
        }
    }
}