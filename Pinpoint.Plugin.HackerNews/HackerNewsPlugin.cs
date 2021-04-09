using System.Collections.Generic;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.HackerNews
{
    public class HackerNewsPlugin : IPlugin
    {
        private const string Description = "Browse the frontpage of Hacker News.\n\nExamples: \"hackernews\", \"hacker news\", \"hnews\"";

        private readonly HackerNewsApi _hackerNewsApi = new HackerNewsApi();

        public PluginMeta Meta { get; set; } = new PluginMeta("Hacker News Browser", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool TryLoad() => true;

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            var raw = query.RawQuery.ToLower();
            return raw.Length >= 10 && (raw.Equals("hackernews") || raw.Equals("hacker news") || raw.Equals("hnews"));
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
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
