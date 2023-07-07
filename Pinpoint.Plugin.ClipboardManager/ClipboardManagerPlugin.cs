using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Clipboard;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.ClipboardManager
{
    public class ClipboardManagerPlugin : IPlugin
    {
        public PluginManifest Manifest { get; set; } = new("Clipboard Manager")
        {
            Description = "Evaluate mathematical expressions quickly.\n\nExamples: \"9+5*(123/5)\""
        };

        public PluginStorage Storage { get; set; } = new();

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query) => false;

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            foreach (var clipboardEntry in ClipboardHelper.History)
            {
                yield return new ClipboardResult(clipboardEntry);
            }
        }
    }
}
