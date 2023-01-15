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
            new PathAppProvider()
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
                        var variations = GenerateAliases(a.Name);
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
                foreach (var alternateName in GenerateAliases(app.Name))
                {
                    _staticAppsTrie.Add(alternateName, app);
                }
            }
        }

        private IEnumerable<string> GenerateAliases(string appName)
        {
            yield return appName.ToLower();
            
            var camelCaseAliases = GenerateCamelCaseAliases(appName);
            foreach (var alias in camelCaseAliases)
            {
                yield return alias;
            }
            
            var wordAliases = GenerateWordAliases(appName);
            foreach (var alias in wordAliases)
            {
                yield return alias;
            }

            var acronymAliases = GenerateAcronymAliases(appName);
            foreach (var alias in acronymAliases)
            {
                yield return alias;
            }
        }

        private IEnumerable<string> GenerateAcronymAliases(string appName)
        {
            var variations = appName.ToLower().Split(' ');
            if (variations.Length <= 1)
            {
                yield break;
            }

            var appAcronymLetters = variations.Where(part => part.Length > 0)
                .Select(part => part[0])
                .ToArray();
            yield return string.Join("", appAcronymLetters);
        }

        private IEnumerable<string> GenerateWordAliases(string appName)
        {
            if (!appName.Contains(' '))
            {
                yield break;
            }
            
            var variations = appName.ToLower().Split(" ");
            foreach (var variation in variations)
            {
                yield return variation;
            }
        }

        private IEnumerable<string> GenerateCamelCaseAliases(string appName)
        {
            if (appName.All(char.IsLower) || appName.All(char.IsUpper))
            {
                yield break;
            }
            
            var subWords = appName.Split(' ');
            foreach (var subWord in subWords)
            {
                var currentVariation = "";
                foreach (var c in subWord)
                {
                    if (char.IsUpper(c) && currentVariation.Length > 1)
                    {
                        yield return currentVariation;
                        currentVariation = c.ToString().ToLower();
                        continue;
                    }
                
                    currentVariation += c;
                }
                
                if (currentVariation.Length > 1)
                {
                    yield return currentVariation.ToLower();
                }
            }
        }
    }
}
