using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Shortcuts
{
    public class ShortcutsPlugin : IPlugin
    {
        public PluginManifest Manifest { get; set; } = new("Shortcuts", PluginPriority.Highest)
        {
            Description = "Create custom shortcuts to open file locations, settings and even websites.\n\nExamples:\nName: youtube; Value: https://youtube.com,\nName: desktop; Value: C:\\Users\\{Environment.UserName}\\Desktop,\nName: apps; Value: ms-settings:appsfeatures, \nName: desktop; Value: C:\\Users\\{Environment.UserName}\\Desktop,\nName: reddit; Value: https://www.reddit.com/r/{{query}}"
        };

        public PluginStorage Storage { get; set; } = new();

        public bool HasModifiableSettings => true;

        public async Task<bool> TryLoad()
        {
            Storage.UserSettings.Put("", "");
            return true;
        }

        public async Task<bool> Activate(Query query)
        {
            return Storage.UserSettings.Any(s => s.Name.Equals(query.Parts[0]));
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, CancellationToken ct)
        {
            if (query.Parts.Length == 1)
            {
                yield return new ShortcutsResult(query.RawQuery, Storage.UserSettings.Str(query.RawQuery));
                yield break;
            }

            var q = query.Parts;
            var shortcut = Storage.UserSettings.Str(q[0]).Replace("{query}", q[1]);
            yield return new ShortcutsResult(q[0], shortcut);
        }
    }
}