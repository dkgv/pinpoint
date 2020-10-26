using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.CommandLine
{
    public class CommandLinePlugin : IPlugin
    {
        public PluginMeta Meta { get; set; } = new PluginMeta("Command Line", PluginPriority.Highest);

        public void Load()
        {
        }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return query.Prefix().Equals(">") && query.RawQuery.Length > 1;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            yield return new CommandLineResult(query.RawQuery);
        }
    }
}
