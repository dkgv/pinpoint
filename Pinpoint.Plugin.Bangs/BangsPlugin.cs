using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Bangs
{
    public class BangsPlugin : IPlugin
    {
        private const string Description = "Search 10.000+ websites directly via DuckDuckGo bangs.\n\nExamples: \"breaking bad !imdb\", \"diameter of the earth !g\"";

        public PluginMeta Meta { get; set; } = new("DuckDuckGo !Bangs", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new();

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            if (query.Parts.Length < 2)
            {
                return false;
            }

            // Matches either "my query !g" or "!g my query" formats
            return query.Parts[0].StartsWith("!") || query.Parts[^1].StartsWith("!");
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            yield return new BangResult($"https://duckduckgo.com/?q={query.RawQuery.Replace(' ', '+')}");
        }
    }
}
