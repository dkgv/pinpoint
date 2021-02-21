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
        private List<Type> _originalPluginTypes = new List<Type>();
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

                _originalPluginTypes.Add(plugin.GetType());

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

                if (await plugin.Activate(query))
                {
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
                            RemovePlugin(plugin);
                            errorOccurred = true;
                        }

                        if (result != null)
                            yield return result;
                    }
                }
                
            }

            if(errorOccurred)
            {
                ReconstructFailedPlugins();
            }
        }

        private void ReconstructFailedPlugins()
        {
            var removedPluginTypes = _originalPluginTypes.Where(pluginType => !Plugins.Any(plugin => plugin.GetType() == pluginType));

            foreach (var pluginType in removedPluginTypes)
            {
                var pluginInstance = Activator.CreateInstance(pluginType) as IPlugin;

                if(pluginInstance != null)
                {
                    AddPlugin(pluginInstance);
                }
            }
        }
    }
}
