using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.AppSearch.Models;
using Pinpoint.Plugin.AppSearch.Providers;

namespace Pinpoint.Plugin.AppSearch;

public class AppSearchPlugin : AbstractPlugin
{
    private static readonly UkkonenTrie<IApp> StaticAppsTrie = new();

    private static readonly IAppProvider[] RuntimeAppProviders = {
        new StandardAppProvider(),
        new PathAppProvider(),
    };

    public AppSearchFrequency AppFrequency;

    public override PluginManifest Manifest { get; } = new("App Search")
    {
        Description = "Search for installed apps. Type an app name or an abbreviation thereof.\n\nExamples: \"visual studio code\", \"vsc\""
    };

    public override async Task<bool> Initialize()
    {
        AppFrequency = new AppSearchFrequency(this);

        await Task.WhenAll(InitializeStaticApps(), InitializeRuntimeApps());

        return true;
    }

    private async Task InitializeStaticApps()
    {
        var uwpProvider = new UwpAppProvider();
        foreach (var app in await uwpProvider.Provide())
        {
            foreach (var alias in GenerateAliases(app.Name))
            {
                StaticAppsTrie.Add(alias, app);
            }
        }
    }

    private async Task InitializeRuntimeApps()
    {
        var tasks = new List<Task>();
        foreach (var provider in RuntimeAppProviders)
        {
            tasks.Add(provider.Provide());
        }

        await Task.WhenAll(tasks);
    }

    public override async Task<bool> ShouldActivate(Query query) => query.Raw.Length >= 2;

    public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
    {
        var queryLower = query.Raw.ToLower();
        var cachedMatches = StaticAppsTrie.Retrieve(queryLower);
        var runtimeMatches = await GetRuntimeMatches(queryLower);

        var allMatches = cachedMatches
            .Concat(runtimeMatches)
            .OrderByDescending(a => AppFrequency.FrequencyOfFor(queryLower, a.FilePath));

        foreach (var app in allMatches)
        {
            yield return new AppResult(this, app, queryLower);
        }
    }

    private async Task<IEnumerable<IApp>> GetRuntimeMatches(string queryLower)
    {
        var runtimeTasks = await Task.WhenAll(RuntimeAppProviders.Select(provider => provider.Provide()));
        return runtimeTasks
            .SelectMany(apps => apps)
            .Where(app => GenerateAliases(app.Name).Any(alias => alias.StartsWith(queryLower)));
    }
    private IEnumerable<string> GenerateAliases(string appName)
    {
        yield return appName.ToLower();

        foreach (var alias in GenerateCamelCaseAliases(appName))
        {
            yield return alias;
        }

        foreach (var alias in GenerateWordAliases(appName))
        {
            yield return alias;
        }

        foreach (var alias in GenerateAcronymAliases(appName))
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

        var words = appName.ToLower().Split(" ");
        foreach (var variation in words)
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
