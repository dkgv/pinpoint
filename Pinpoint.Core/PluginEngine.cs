using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core.Results;

namespace Pinpoint.Core;

public class PluginEngine
{
    public List<AbstractPlugin> Plugins { get; } = new();

    public List<IPluginListener<AbstractPlugin, object>> Listeners { get; } = new();

    public async Task LoadPlugin(AbstractPlugin plugin)
    {
        if (Plugins.Contains(plugin))
        {
            return;
        }

        try
        {
            await plugin.Initialize();
        }
        catch
        {
            // Plugin threw an exception while loading, so don't add it.
            return;
        }

        Plugins.Add(plugin);
        Plugins.Sort();

        Listeners.ForEach(listener => listener.PluginChange_Added(this, plugin, null));
    }

    public T GetPluginByType<T>() where T : AbstractPlugin => Plugins.Where(p => p is T).Cast<T>().FirstOrDefault();

    public async IAsyncEnumerable<AbstractQueryResult> EvaluateQuery([EnumeratorCancellation] CancellationToken ct,
        Query query)
    {
        var enabledPlugins = Plugins.Where(p => p.State.IsEnabled.GetValueOrDefault(false)).ToList();
        var debouncedPlugins = enabledPlugins.Where(p => p.State.DebounceTime > TimeSpan.Zero).ToHashSet();
        for (var i = 0; i < enabledPlugins.Count && query.ResultCount < 20 && !ct.IsCancellationRequested; i++)
        {
            var plugin = enabledPlugins[i];
            if (!await plugin.ShouldActivate(query))
            {
                continue;
            }

            if (debouncedPlugins.Contains(plugin))
            {
                await foreach (var result in Debounce(ct, plugin, query))
                {
                    query.ResultCount++;
                    yield return result;
                }
            }
            else
            {
                await foreach (var result in plugin.ProcessQuery(query, ct))
                {
                    query.ResultCount++;
                    yield return result;
                }
            }
        }
    }

    private async IAsyncEnumerable<AbstractQueryResult> Debounce([EnumeratorCancellation] CancellationToken ct, AbstractPlugin plugin, Query query)
    {
        await Task.Delay(plugin.State.DebounceTime, ct);

        if (ct.IsCancellationRequested)
        {
            yield break;
        }

        await foreach (var result in plugin.ProcessQuery(query, ct))
        {
            query.ResultCount++;
            yield return result;
        }
    }
}
