﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.AppSearch.Providers;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppSearchPlugin : AbstractPlugin
    {
        private static readonly UkkonenTrie<IApp> CachedAppsTrie = new();
        private static readonly IAppProvider StandardAppProvider = new StandardAppProvider();
        private static readonly IAppProvider PathAppProvider = new PathAppProvider();
        private static readonly IAppProvider[] RuntimeAppProviders = {
            StandardAppProvider,
            PathAppProvider,
        };

        public AppSearchFrequency AppSearchFrequency;

        public override PluginManifest Manifest { get; } = new("App Search")
        {
            Description = "Search for installed apps. Type an app name or an abbreviation thereof.\n\nExamples: \"visual studio code\", \"vsc\""
        };

        public override async Task<bool> Initialize()
        {
            AppSearchFrequency = new AppSearchFrequency(this);

            var tasks = new List<Task>
            {
                Task.Run(() => PopulateCache(new UwpAppProvider())),
                Task.Run(() => PathAppProvider.Provide().ToList()),
                Task.Run(() => StandardAppProvider.Provide().ToList()),
            };
            await Task.WhenAll(tasks);

            return true;
        }

        public override async Task<bool> ShouldActivate(Query query) => query.Raw.Length >= 2;

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var queryLower = query.Raw.ToLower();

            var cachedMatches = CachedAppsTrie.Retrieve(queryLower);
            var runtimeMatches = RuntimeAppProviders.SelectMany(provider => provider.Provide())
                    .Where(a =>
                    {
                        var variations = GenerateAliases(a.Name);
                        return variations.Any(v => v.StartsWith(queryLower));
                    });
            var allMatches = cachedMatches
                .Concat(runtimeMatches)
                .OrderByDescending(a => AppSearchFrequency.FrequencyOfFor(queryLower, a.FilePath));

            foreach (var app in allMatches)
            {
                yield return new AppResult(this, app, queryLower);
            }
        }

        private void PopulateCache(IAppProvider provider)
        {
            foreach (var app in provider.Provide())
            {
                foreach (var alternateName in GenerateAliases(app.Name))
                {
                    CachedAppsTrie.Add(alternateName, app);
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
