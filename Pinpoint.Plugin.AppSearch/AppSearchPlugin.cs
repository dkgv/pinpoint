using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppSearchPlugin : IPlugin
    {
        private UkkonenTrie<IApp> _trie = new();

        private static readonly IAppProvider[] AppProviders = {
            new StandardAppProvider(),
            new UwpAppProvider()
        };

        private const string Description = "Search for installed apps. Type an app name or an abbreviation thereof.\n\nExamples: \"visual studio code\", \"vsc\"";

        public PluginMeta Meta { get; set; } = new("App Search", Description, PluginPriority.NextHighest);

        public PluginSettings UserSettings { get; set; } = new();

        public bool IsLoaded { get; set; }

        public async Task<bool> TryLoad()
        {
            AppSearchFrequency.Load();

            foreach (var provider in AppProviders)
            {
                PopulateFromProvider(provider);
            }

            return IsLoaded = true;
        }

        public void Unload()
        {
            _trie = null;
            AppSearchFrequency.Reset();
        }

        public async Task<bool> Activate(Query query) => query.RawQuery.Length >= 2;

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var rawQuery = query.RawQuery.ToLower();

            foreach (var app in _trie.Retrieve(rawQuery)
                .OrderByDescending(entry => AppSearchFrequency.FrequencyOfFor(rawQuery, entry.FilePath)))
            {
                yield return new AppResult(app, rawQuery);
            }
        }

        private void PopulateFromProvider(IAppProvider provider)
        {
            foreach (var app in provider.Provide())
            {
                var appName = app.Name.ToLower();
                _trie.Add(appName, app);

                if (!app.Name.Contains(" "))
                {
                    continue;
                }

                // Support search for "Mozilla Firefox" through both "Mozilla" and "Firefox"
                var variations = appName.Split(" ");
                foreach (var variation in variations)
                {
                    _trie.Add(variation, app);
                }

                // Support "visual studio code" -> "vsc"
                if (variations.Length > 1)
                {
                    var appAcronymLetters = variations.Where(part => part.Length > 0)
                        .Select(part => part[0])
                        .ToArray();
                    var acronym = string.Join(',', appAcronymLetters).Replace(",", "");
                    _trie.Add(acronym, app);
                }
            }
        }
    }
}
