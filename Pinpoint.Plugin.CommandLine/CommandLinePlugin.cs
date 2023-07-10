using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.CommandLine
{
    public class CommandLinePlugin : AbstractPlugin
    {
        public override PluginManifest Manifest { get; } = new("Command Line", PluginPriority.High)
        {
            Description = "Convert colors from RGB <-> Hex.\n\nExamples: \"#FF5555\", \"rgb(255,100,50)\""
        };

        public override async Task<bool> ShouldActivate(Query query)
        {
            return query.Prefix().Equals(">") && query.RawQuery.Length > 1;
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            yield return new CommandLineResult(query.RawQuery);
        }
    }
}
