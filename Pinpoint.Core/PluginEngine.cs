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
        private List<IPlugin> _failedPlugins = new List<IPlugin>();
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
            var errorOccurred = false;

            foreach (var plugin in Plugins.Where(p => p.Meta.Enabled))
            {
                if (numResults >= 30)
                {
                    yield break;
                }

                if (!await plugin.Activate(query)) continue;

                var resultStream = plugin.Process(query).WithCancellation(ct);

                var enumerator = resultStream.GetAsyncEnumerator();
                var hasResult = true;
                AbstractQueryResult result = null;

                while(hasResult)
                {
                    try
                    {
                        hasResult = await enumerator.MoveNextAsync();
                        if (hasResult)
                        {
                            result = enumerator.Current;
                            numResults++;
                        }
                        else
                            result = null;
                    }
                    catch (Exception e)
                    {
                        await ErrorLogging.LogException(e);
                        errorOccurred = true;
                        _failedPlugins.Add(plugin);
                    }

                    if (result != null)
                        yield return result;
                }

            }

            if(errorOccurred)
            {
                ReconstructFailedPlugins();
            }
        }

        private void ReconstructFailedPlugins()
        {
            foreach (var failedPlugin in _failedPlugins)
            {
                RemovePlugin(failedPlugin);

                var newPlugin = Activator.CreateInstance(failedPlugin.GetType()) as IPlugin;
                AddPlugin(newPlugin);
            }

            _failedPlugins.Clear();
        }
    }
}
