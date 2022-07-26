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
        private static readonly string Description = $"Create custom shortcuts to open file locations, settings and even websites.\n\nExamples:\nName: youtube; Value: https://youtube.com,\nName: desktop; Value: C:\\Users\\{Environment.UserName}\\Desktop,\nName: apps; Value: ms-settings:appsfeatures, \nName: desktop; Value: C:\\Users\\{Environment.UserName}\\Desktop,\nName: reddit; Value: https://www.reddit.com/r/{{query}}";

        public PluginMeta Meta { get; set; } = new("Shortcuts", Description, PluginPriority.Highest);
        
        public PluginStorage Storage { get; set; } = new();

        public bool ModifiableSettings => true;

        public bool IsLoaded { get; set; }

        public async Task<bool> TryLoad()
        {
            Storage.UserSettings.Put("", "");
            return IsLoaded = true;
        }

        public async Task<bool> Activate(Query query)
        {
            if (query.Parts.Length == 2)
            {
                return Storage.UserSettings.Any(s => s.Name.Equals(query.Parts[0]));
            }
            
            return Storage.UserSettings.Any(s => s.Name.Equals(query.RawQuery) && !s.Value.ToString()!.Contains("query"));
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, CancellationToken ct)
        {
            ShortcutsResult result;
            if (query.Parts.Length == 1)
            {
                result = new ShortcutsResult(query.RawQuery, Storage.UserSettings.Str(query.RawQuery));
            }
            else
            {
                var q = query.Parts;
                result = new ShortcutsResult(q[0], Storage.UserSettings.Str(q[0]).Replace("{query}", q[1]));
            }

            yield return result;
        }
    }
}