using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Newtonsoft.Json;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppSearchPlugin : IPlugin
    {
        public AppSearchFrequency AppSearchFrequency;

        private static readonly IAppProvider[] AppProviders = {
            new StandardAppProvider(),
            new UwpAppProvider()
        };

        public AppSearchPlugin()
        {
            AppSearchFrequency = new AppSearchFrequency(this);
        }

        private const string Description = "Search for installed apps. Type an app name or an abbreviation thereof.\n\nExamples: \"visual studio code\", \"vsc\"";

        public PluginMeta Meta { get; set; } = new("App Search", Description, PluginPriority.NextHighest);

        public PluginStorage Storage { get; set; } = new();

        public bool IsLoaded { get; set; }

        public async Task<bool> TryLoad()
        {
            if (Storage.InternalSettings.ContainsKey("database"))
            {
                AppSearchFrequency.Database =
                    JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(Storage.InternalSettings["database"].ToString());
            }

            return IsLoaded = true;
        }

        public void Unload()
        {
            AppSearchFrequency.Reset();
        }

        public async Task<bool> Activate(Query query) => query.RawQuery.Length >= 2;

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var trie = new UkkonenTrie<IApp>();
            foreach (var provider in AppProviders)
            {
                PopulateFromProvider(trie, provider);
            }
            
            var rawQuery = query.RawQuery.ToLower();
            foreach (var app in trie.Retrieve(rawQuery)
                .OrderByDescending(entry =>AppSearchFrequency.FrequencyOfFor(rawQuery, entry.FilePath)))
            {
                yield return new AppResult(this, app, rawQuery);
            }
        }

        private void PopulateFromProvider(ITrie<IApp> trie, IAppProvider provider)
        {
            foreach (var app in provider.Provide())
            {
                var appName = app.Name.ToLower();
                trie.Add(appName, app);

                if (!app.Name.Contains(" "))
                {
                    continue;
                }

                // Support search for "Mozilla Firefox" through both "Mozilla" and "Firefox"
                var variations = appName.Split(" ");
                foreach (var variation in variations)
                {
                    trie.Add(variation, app);
                }

                // Support "visual studio code" -> "vsc"
                if (variations.Length <= 1)
                {
                    continue;
                }
                
                var appAcronymLetters = variations.Where(part => part.Length > 0)
                    .Select(part => part[0])
                    .ToArray();
                var acronym = string.Join(',', appAcronymLetters).Replace(",", "");
                trie.Add(acronym, app);
            }
        }
    }
}
