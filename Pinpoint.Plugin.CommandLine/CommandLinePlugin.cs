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
        public PluginManifest Manifest { get; set; } = new("Command Line", PluginPriority.Highest)
        {
            Description = "Convert colors from RGB <-> Hex.\n\nExamples: \"#FF5555\", \"rgb(255,100,50)\""
        };

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
