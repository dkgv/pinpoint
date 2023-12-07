using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core.Clipboard;

namespace Pinpoint.Plugin.ClipboardManager
{
    public class ClipboardManagerPlugin : AbstractPlugin
    {
        public override PluginManifest Manifest { get; } = new("Clipboard Manager")
        {
            Description = "Evaluate mathematical expressions quickly.\n\nExamples: \"9+5*(123/5)\""
        };

        public override async Task<bool> ShouldActivate(Query query) => false;

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            foreach (var clipboardEntry in ClipboardHelper.History)
            {
                yield return new ClipboardResult(clipboardEntry);
            }
        }
    }
}
