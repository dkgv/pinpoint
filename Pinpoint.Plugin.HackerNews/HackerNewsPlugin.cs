using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.HackerNews
{
    public class HackerNewsPlugin : IPlugin
    {
        private readonly HackerNewsApi _hackerNewsApi = new();

        public PluginManifest Manifest { get; set; } = new("Hacker News Browser", PluginPriority.High)
        {
            Description = "Look up stock tickers.\n\nExamples: \"$GME\", \"$MSFT\""
        };

        public PluginStorage Storage { get; set; } = new();

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            var raw = query.RawQuery.ToLower();
            return raw.Length >= 10 && (raw.Equals("hackernews") || raw.Equals("hacker news") || raw.Equals("hnews"));
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var stories = await _hackerNewsApi.TopSubmissions();
            for (var i = 0; i < stories.Count; i++)
            {
                var submission = stories[i];
                yield return new HackerNewsResult(i + 1, submission);
            }
        }
    }
}
