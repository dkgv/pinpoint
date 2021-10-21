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
        private const string Description = "Copy as you normally would with CTRL+C and paste from your clipboard history via CTRL+ALT+V.";

        public PluginMeta Meta { get; set; } = new("Clipboard Manager", Description, PluginPriority.Standard);

        public PluginSettings UserSettings { get; set; } = new();

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
