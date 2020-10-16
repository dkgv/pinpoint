using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin
{
    public class PluginEngine
    {
        public List<IPlugin> Plugins { get; } = new List<IPlugin>();

        public List<IPluginListener<IPlugin, object>> Listeners { get; } = new List<IPluginListener<IPlugin, object>>();

        public void AddPlugin(IPlugin plugin)
        {
            var prevSettings = AppSettings.GetListAs<PluginMeta>("plugins");

            if (!Plugins.Contains(plugin))
            {
                plugin.Load();

                var pluginName = plugin.Meta.Name;
                if (prevSettings.Any(m => m.Name.Equals(pluginName)))
                {
                    plugin.Meta = prevSettings.First(m => m.Name.Equals(pluginName));
                }

                Plugins.Add(plugin);
                Listeners.ForEach(listener => listener.PluginChange_Added(this, plugin, null));
            }
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

        public T Plugin<T>() where T : IPlugin
        {
            return Plugins.Where(p => p is T).Cast<T>().FirstOrDefault();
        }

        public async IAsyncEnumerable<IQueryResult> Process(Query query, CancellationToken ct)
        {
            var results = 0;

            foreach (var plugin in Plugins.Where(p => p.Meta.Enabled))
            {
                if (results >= 30)
                {
                    yield break;
                }

                if (await plugin.Activate(query))
                {
                    await foreach (var result in plugin.Process(query).WithCancellation(ct))
                    {
                        results++;
                        yield return result;
                    }
                }
            }
        }
    }
}
