using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core.Results;

namespace Pinpoint.Core
{
    public class PluginEngine
    {
        public List<IPlugin> Plugins { get; } = new();

        public List<IPluginListener<IPlugin, object>> Listeners { get; } = new();

        public async Task AddPlugin(IPlugin @new)
        {
            @new.Restore();

            // Ensure plugin hasn't been added and that it was able to load
            if (Plugins.Contains(@new))
            {
                return;
            }

            try
            {
                var pluginLoaded = await @new.TryLoad();

                if (!pluginLoaded)
                {
                    return;
                }
            }   
            catch
            {
                // Plugin threw an exception while loading, so don't add it.
                return;
            }
            
            Plugins.Add(@new);

            Listeners.ForEach(listener => listener.PluginChange_Added(this, @new, null));
        }

        public T GetPluginByType<T>() where T : IPlugin => Plugins.Where(p => p is T).Cast<T>().FirstOrDefault();

        public async Task<List<AbstractQueryResult>> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var activePlugins = new BlockingCollection<IPlugin>();
            _ = Task.Run(() =>
            {
                Parallel.ForEach(Plugins.Where(p => p.Meta.Enabled && p.IsLoaded), async plugin =>
                {
                    if (await plugin.Activate(query))
                    {
                        activePlugins.Add(plugin, ct);
                    }
                });

                activePlugins.CompleteAdding();
            }, ct);

            var results = new List<AbstractQueryResult>(20);
            var tasks = new List<Task>();
            while (activePlugins.TryTake(out var plugin, TimeSpan.FromSeconds(1)) && query.ResultCount < 20)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await foreach (var result in plugin.Process(query, ct))
                    {
                        query.ResultCount++;
                        lock (results)
                        {
                            results.Add(result);
                        }
                    }
                }, ct));
            }

            await Task.WhenAll(tasks);

            return results;
        }
    }
}
