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

        public async Task AddPlugin(IPlugin toAdd)
        {
            // Disallow duplicate plugins
            if (Plugins.Contains(toAdd))
            {
                return;
            }

            var pluginCouldLoad = await toAdd.TryLoad();
            // Don't add plugin if it fails to load
            if (!pluginCouldLoad)
            {
                return;
            }

            var plugins = AppSettings.GetOrDefault("plugins", new EmptyPlugin[0]);

            var match = plugins.FirstOrDefault(p => p.Meta.Name.Equals(toAdd.Meta.Name));

            if (match != null)
            {
                toAdd.UserSettings = match.UserSettings;
                toAdd.Meta.Enabled = match.Meta.Enabled;
            }

            Plugins.Add(toAdd);

            // Ensure order of plugin execution is correct
            Plugins.Sort();

            Listeners.ForEach(listener => listener.PluginChange_Added(this, toAdd, null));
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

        public IPlugin PluginByType(Type type) => Plugins.First(plugin => plugin.GetType() == type);

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var enabledPlugins = Plugins.Where(p => p.Meta.Enabled && p.IsLoaded).ToList();

            for (int i = 0, numResults = 0; i < enabledPlugins.Count && numResults < 20 && !ct.IsCancellationRequested; i++)
            {
                var plugin = enabledPlugins[i];
                if (await plugin.Activate(query))
                {
                    await foreach (var result in plugin.Process(query).WithCancellation(ct))
                    {
                        numResults++;
                        yield return result;
                    }
                }
            }
        }
    }
}
