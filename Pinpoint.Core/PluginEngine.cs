using System;
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
        public List<IPlugin> Plugins { get; } = new List<IPlugin>();

        public List<IPluginListener<IPlugin, object>> Listeners { get; } = new List<IPluginListener<IPlugin, object>>();

        public async Task AddPlugin(IPlugin @new)
        {
            // Ensure plugin hasn't been added and that it was able to load
            if (Plugins.Contains(@new) || !await @new.TryLoad())
            {
                return;
            }

            var plugins = AppSettings.GetOrDefault("plugins", Array.Empty<EmptyPlugin>());
            var match = plugins.FirstOrDefault(p => p.Meta.Name.Equals(@new.Meta.Name));
            if (match != null)
            {
                @new.UserSettings = match.UserSettings;
                @new.Meta.Enabled = match.Meta.Enabled;
            }

            // Ensure order of plugin execution is correct
            Plugins.Add(@new);
            Plugins.Sort();

            Listeners.ForEach(listener => listener.PluginChange_Added(this, @new, null));
        }

        public void RemovePlugin(IPlugin plugin)
        {
            if (Plugins.Contains(plugin))
            {
                plugin.Unload();
                Plugins.Remove(plugin);
                Listeners.ForEach(listener => listener.PluginChange_Removed(this, plugin, null));
            }
        }

        public T PluginByType<T>() where T : IPlugin => Plugins.Where(p => p is T).Cast<T>().FirstOrDefault();

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var enabledPlugins = Plugins.Where(p => p.Meta.Enabled && p.IsLoaded).ToList();
            for (int i = 0; i < enabledPlugins.Count && query.ResultCount < 20 && !ct.IsCancellationRequested; i++)
            {
                var plugin = enabledPlugins[i];
                if (!await plugin.Activate(query))
                {
                    continue;
                }

                await foreach (var result in plugin.Process(query, ct))
                {
                    query.ResultCount++;
                    yield return result;
                }
            }
        }
    }
}
