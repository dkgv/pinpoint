using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.UrlLauncher
{
    public class UrlLauncherPlugin: IPlugin
    {
        private const string HttpsProtocolPrefix = "https://";
        private static readonly List<string> Tlds = new List<string>(1577);

        public PluginMeta Meta { get; set; } = new PluginMeta("Url launcher", PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool TryLoad()
        {
            Tlds.AddRange(File.ReadAllLines("tlds.txt"));
            return true;
        }

        public void Unload() { }

        public async Task<bool> Activate(Query query)
        {
            if (query.RawQuery.Length < 3 || !query.RawQuery.Contains("."))
            {
                return false;
            }
            var split = query.RawQuery.Split(".");
            return split.Length > 0 && Tlds.Contains(split[^1]);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var url = query.RawQuery;
            if (!query.RawQuery.StartsWith(HttpsProtocolPrefix))
            {
                url = HttpsProtocolPrefix + url;
            }

            var uri = new Uri(url);
            yield return new UrlLauncherResult(uri.ToString());
        }
    }
}
