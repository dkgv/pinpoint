using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Bangs
{
    public class BangsPlugin : AbstractPlugin
    {
        public override PluginManifest Manifest { get; } = new("DuckDuckGo !Bangs", PluginPriority.High)
        {
            Description = "Search for installed apps. Type an app name or an abbreviation thereof.\n\nExamples: \"visual studio code\", \"vsc\""
        };

        public override async Task<bool> ShouldActivate(Query query)
        {
            if (query.Parts.Length < 2)
            {
                return false;
            }

            // Matches either "my query !g" or "!g my query" formats
            return query.Parts[0].StartsWith("!") || query.Parts[^1].StartsWith("!");
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            yield return new BangResult($"https://duckduckgo.com/?q={query.RawQuery.Replace(' ', '+')}");
        }
    }
}
