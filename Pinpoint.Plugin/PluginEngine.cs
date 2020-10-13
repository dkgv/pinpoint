using System.Collections.Generic;
using System.Linq;

namespace Pinpoint.Plugin
{
    public class PluginEngine : IPluginListener<IPlugin, object>
    {
        public static readonly List<IPlugin> Plugins = new List<IPlugin>();

        public void AddPlugin(IPlugin plugin)
        {
            var prevSettings = AppSettings.GetListAs<PluginMeta>("plugins");

            if (!Plugins.Contains(plugin))
            {
                plugin.Load();

                if (prevSettings.Any())
                {
                    plugin.Meta = prevSettings.First(meta => meta.Name.Equals(plugin.Meta.Name));
                }

                Plugins.Add(plugin);
            }
        }

        public void RemovePlugin(IPlugin plugin)
        {
            plugin.Unload();
            Plugins.Remove(plugin);
        }

        public T Plugin<T>() where T : IPlugin
        {
            return Plugins.Where(p => p is T).Cast<T>().FirstOrDefault();
        }

        public async IAsyncEnumerable<IQueryResult> Process(Query query)
        {
            foreach (var plugin in Plugins.Where(p => p.Meta.Enabled))
            {
                if (await plugin.Activate(query))
                {
                    await foreach (var result in plugin.Process(query))
                    {
                        yield return result;
                    }
                }
            }
        }

        public void PluginChange_Added(object sender, IPlugin plugin, object target)
        {
            throw new System.NotImplementedException();
        }

        public void PluginChange_Removed(object sender, IPlugin plugin, object target)
        {
            throw new System.NotImplementedException();
        }
    }
}
