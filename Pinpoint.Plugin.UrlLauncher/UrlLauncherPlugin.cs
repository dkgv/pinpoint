using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.UrlLauncher
{
    public class UrlLauncherPlugin: IPlugin
    {
        private const string HttpsProtocolPrefix = "https://";
        private static readonly HashSet<string> Tlds = new(1577);
        private const string Description = "Launch URLs.\n\nExamples: \"google.com\", \"images.google.com\"";

        public PluginMeta Meta { get; set; } = new("Url Launcher", Description, PluginPriority.Highest);

        public PluginStorage Storage { get; set; } = new();

        public bool IsLoaded { get; set; }

        public Task<bool> TryLoad()
        {
            var asm = GetType().Assembly;

            using var stream = asm.GetManifestResourceStream("Pinpoint.Plugin.UrlLauncher.tlds.txt");
            using var reader = new StreamReader(stream, Encoding.UTF8);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Tlds.Add(line);
            }

            IsLoaded = true;
            return Task.FromResult(IsLoaded);
        }

        public void Unload() { }

        public async Task<bool> Activate(Query query)
        {
            if (query.RawQuery.Length < 3 || query.RawQuery[0] == '.' || !query.RawQuery.Contains("."))
            {
                return false;
            }

            var split = query.RawQuery.Split(".");
            return split.Length > 0 && Tlds.Contains(split[^1]);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var url = query.RawQuery;
            if (!query.RawQuery.StartsWith(HttpsProtocolPrefix))
            {
                url = HttpsProtocolPrefix + url;
            }

            Uri uri = null;
            try
            {
                uri = new Uri(url);
            }
            catch (UriFormatException)
            {
                // invalid hostname
            }

            if (uri != null)
            {
                yield return new UrlLauncherResult(uri.ToString());
            }
        }
    }
}
