using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.ClipboardManager
{
    public class ClipboardManagerPlugin : IPlugin
    {
        public readonly ClipboardHistory ClipboardHistory = new ClipboardHistory();

        public PluginMeta Meta { get; set; } = new PluginMeta("Clipboard Manager", PluginPriority.Standard);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool TryLoad() => true;

        public void Unload()
        {
            ClipboardHistory.Clear();
        }

        public async Task<bool> Activate(Query query)
        {
            return query.RawQuery.StartsWith("clip");
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            foreach (var clipboardEntry in ClipboardHistory)
            {
                yield return new ClipboardResult(clipboardEntry);
            }
        }
    }
}
