using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.HackerNews
{
    public class HackerNewsPlugin : AbstractPlugin
    {
        private readonly HackerNewsApi _hackerNewsApi = new();

        public override PluginManifest Manifest { get; } = new("Hacker News Browser", PluginPriority.High)
        {
            Description = "Look up stock tickers.\n\nExamples: \"$GME\", \"$MSFT\""
        };

        public override async Task<bool> ShouldActivate(Query query)
        {
            var raw = query.Raw.ToLower();
            return raw.Length >= 10 && (raw.Equals("hackernews") || raw.Equals("hacker news") || raw.Equals("hnews"));
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
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
