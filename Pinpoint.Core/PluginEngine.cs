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

                // Ensure order of plugin execution is correct
                Plugins.Sort();

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

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var numResults = 0;
            var deadPlugins = new List<IPlugin>();

            foreach (var plugin in Plugins.Where(p => p.Meta.Enabled))
            {
                if (numResults >= 30)
                {
                    yield break;
                }

                if (!await plugin.Activate(query))
                {
                    continue;
                }

                var enumerator = plugin.Process(query).WithCancellation(ct).GetAsyncEnumerator();
                AbstractQueryResult result = null;

                do
                {
                    try
                    {
                        if (await enumerator.MoveNextAsync())
                        {
                            result = enumerator.Current;
                            numResults++;
                        }
                        else
                        {
                            result = null;
                        }
                    }
                    catch (Exception e)
                    {
                        await ErrorLogging.LogException(e);
                        deadPlugins.Add(plugin);
                    }

                    if (result != null)
                    {
                        yield return result;
                    }
                } while (result != null);
            }

            if (deadPlugins.Any())
            {
                ReinitializePlugins(deadPlugins);
            }
        }

        private void ReinitializePlugins(List<IPlugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                RemovePlugin(plugin);

                var newPlugin = Activator.CreateInstance(plugin.GetType()) as IPlugin;
                AddPlugin(newPlugin);
            }
        }
    }
}
