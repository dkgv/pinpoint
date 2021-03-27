using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.UrlLauncher
{
    public class UrlLauncherPlugin: IPlugin
    {
        private const string HttpsProtocolPrefix = "https://";
        private static readonly HashSet<string> Tlds = new HashSet<string>(1577);

        public PluginMeta Meta { get; set; } = new PluginMeta("Url launcher", PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool TryLoad()
        {
            var asm = GetType().Assembly;

            using var stream = asm.GetManifestResourceStream("Pinpoint.Plugin.UrlLauncher.tlds.txt");
            using var reader = new StreamReader(stream, Encoding.UTF8);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Tlds.Add(line);
            }

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
