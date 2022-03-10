using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.CommandLine
{
    public class CommandLinePlugin : IPlugin
    {
        private const string Description = "Launch CLI programs directly in cmd.\n\nExamples: \">ipconfig\"";

        public PluginMeta Meta { get; set; } = new("Command Line", Description, PluginPriority.Highest);

        public PluginStorage Storage { get; set; } = new();

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return query.Prefix().Equals(">") && query.RawQuery.Length > 1;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            yield return new CommandLineResult(query.RawQuery);
        }
    }
}
