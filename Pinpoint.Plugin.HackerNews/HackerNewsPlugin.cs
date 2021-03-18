using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.HackerNews
{
    public class HackerNewsPlugin : IPlugin
    {
        private readonly HackerNewsApi _hackerNewsApi = new HackerNewsApi();

        public PluginMeta Meta { get; set; } = new PluginMeta("Hacker News Browser", PluginPriority.Highest);

        public List<PluginSetting> Settings { get; set; } = new List<PluginSetting>();

        public bool TryLoad() => true;

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            var raw = query.RawQuery.ToLower();
            return raw.Equals("hackernews") || raw.Equals("hacker news");
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
