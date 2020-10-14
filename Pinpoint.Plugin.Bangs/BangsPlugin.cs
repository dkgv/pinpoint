﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Bangs
{
    public class BangsPlugin : IPlugin
    {
        public PluginMeta Meta { get; set; } = new PluginMeta("DuckDuckGo !Bangs");

        public void Load()
        {
        }

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

        public async IAsyncEnumerable<IQueryResult> Process(Query query)
        {
            yield return new BangResult($"https://duckduckgo.com/?q={query.RawQuery.Replace(' ', '+')}");
        }
    }
}
