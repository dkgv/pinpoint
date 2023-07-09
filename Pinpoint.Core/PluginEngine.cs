using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core.Results;

namespace Pinpoint.Core;

public class PluginEngine
{
    public List<AbstractPlugin> Plugins { get; } = new();

    public List<AbstractPlugin> LocalPlugins { get; } = new();

    public List<IPluginListener<AbstractPlugin, object>> Listeners { get; } = new();

    public async Task<bool> LoadPlugin(AbstractPlugin plugin)
    {
        if (Plugins.Contains(plugin))
        {
            return true;
        }

        try
        {
            await plugin.Initialize();
        }
        catch
        {
            // Plugin threw an exception while loading, so don't add it.
            return false;
        }

        Plugins.Add(plugin);
        Plugins.Sort();

        Listeners.ForEach(listener => listener.PluginChange_Added(this, plugin, null));

        return true;
    }

    public void Unload(AbstractPlugin plugin)
    {
        Plugins.Remove(plugin);
        LocalPlugins.Remove(plugin);
        Listeners.ForEach(listener => listener.PluginChange_Removed(this, plugin, null));
    }

    public async Task<List<Exception>> LoadLocalPlugins(string directory)
    {
        var path = new DirectoryInfo(directory);
        var dlls = path.GetFiles("*.dll", SearchOption.TopDirectoryOnly);
        var exceptions = new List<Exception>();

        foreach (var dll in dlls)
        {
            var dllFilePath = dll.FullName;
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(dllFilePath);
            }
            catch (BadImageFormatException)
            {
                continue;
            }

            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsSubclassOf(typeof(AbstractPlugin)))
                {
                    continue;
                }

                try
                {
                    var plugin = (AbstractPlugin)Activator.CreateInstance(type);
                    if (await LoadPlugin(plugin))
                    {
                        LocalPlugins.Add(plugin);
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
        }

        return exceptions;
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
