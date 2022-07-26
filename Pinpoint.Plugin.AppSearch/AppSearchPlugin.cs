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
        private UkkonenTrie<IApp> _staticAppsTrie = new();

        public AppSearchFrequency AppSearchFrequency;

        private static readonly IAppProvider[] RuntimeAppProviders = {
            new StandardAppProvider(),
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
            
            PopulateFromProvider(new UwpAppProvider());

            return IsLoaded = true;
        }

        public void Unload()
        {
            _staticAppsTrie = new UkkonenTrie<IApp>();
            AppSearchFrequency.Reset();
        }

        public async Task<bool> Activate(Query query) => query.RawQuery.Length >= 2;

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var queryLower = query.RawQuery.ToLower();

            var staticAppMatches = _staticAppsTrie.Retrieve(queryLower);

            var runtimeAppMatches = RuntimeAppProviders.SelectMany(provider => provider.Provide())
                    .Where(a =>
                    {
                        var variations = GenerateNamesFor(a.Name.ToLower());
                        return variations.Any(v => v.StartsWith(queryLower));
                    });

           foreach (var app in staticAppMatches.Concat(runtimeAppMatches)
                        .OrderByDescending(a => AppSearchFrequency.FrequencyOfFor(queryLower, a.FilePath)))
           {
               yield return new AppResult(this, app, queryLower);
           }
        }

        private void PopulateFromProvider(IAppProvider provider)
        {
            foreach (var app in provider.Provide())
            {
                var appName = app.Name.ToLower();
                _staticAppsTrie.Add(appName, app);

                if (!app.Name.Contains(" "))
                {
                    continue;
                }

                foreach (var alternateName in GenerateNamesFor(appName))
                {
                    _staticAppsTrie.Add(alternateName, app);
                }
            }
        }

        private IEnumerable<string> GenerateNamesFor(string appName)
        {
            yield return appName;

            if (!appName.Contains(" "))
            {
                yield break;
            }
           
            // Support search for "Mozilla Firefox" through both "Mozilla" and "Firefox"
            var variations = appName.Split(" ");
            foreach (var variation in variations)
            {
                yield return variation;
            } 
            
            // Support "visual studio code" -> "vsc"
            if (variations.Length <= 1)
            {
                yield break;
            }

            var appAcronymLetters = variations.Where(part => part.Length > 0)
                .Select(part => part[0])
                .ToArray();
            yield return string.Join("", appAcronymLetters);;
        }
    }
}
