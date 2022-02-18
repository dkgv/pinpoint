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
        private static readonly string Description = $"Create custom shortcuts to open file locations, settings and even websites.\n\nExamples:\nName: youtube; Value: https://youtube.com,\nName: desktop; Value: C:\\Users\\{Environment.UserName}\\Desktop,\nName: apps; Value: ms-settings:appsfeatures";

        public PluginMeta Meta { get; set; } = new("Shortcuts", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new();

        public bool ModifiableSettings { get; set; }

        public bool IsLoaded { get; set; }

        public async Task<bool> TryLoad()
        {
            ModifiableSettings = true;
            UserSettings.Put("", "");
            return IsLoaded = true;
        }

        public async Task<bool> Activate(Query query)
        {
            return UserSettings.Any(setting => setting.Name.Equals(query.RawQuery));
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, CancellationToken ct)
        {
            yield return new ShortcutsResult(query.RawQuery, UserSettings.Str(query.RawQuery));
        }
    }
}