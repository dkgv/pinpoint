using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Shortcuts
{
    public class ShortcutsPlugin : AbstractPlugin
    {
        public override PluginManifest Manifest { get; } = new("Shortcuts", PluginPriority.High)
        {
            Description = "Create custom shortcuts to open file locations, settings and even websites.\n\nExamples:\nName: youtube; Value: https://youtube.com,\nName: desktop; Value: C:\\Users\\{Environment.UserName}\\Desktop,\nName: apps; Value: ms-settings:appsfeatures, \nName: desktop; Value: C:\\Users\\{Environment.UserName}\\Desktop,\nName: reddit; Value: https://www.reddit.com/r/{{query}}"
        };

        public override PluginState State => new()
        {
            HasModifiableSettings = true
        };

        public override PluginStorage Storage => new()
        {
            User = new UserSettings{
                {"", ""}
            }
        };

        public override async Task<bool> ShouldActivate(Query query)
        {
            return Storage.User.Any(s => s.Key.Equals(query.Parts[0]));
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, CancellationToken ct)
        {
            if (query.Parts.Length == 1)
            {
                yield return new ShortcutsResult(query.RawQuery, Storage.User.Str(query.RawQuery));
                yield break;
            }

            var q = query.Parts;
            var shortcut = Storage.User.Str(q[0]).Replace("{query}", q[1]);
            yield return new ShortcutsResult(q[0], shortcut);
        }
    }
}