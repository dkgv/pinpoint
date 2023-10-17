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
    public class UrlLauncherPlugin : AbstractPlugin
    {
        private const string HttpsProtocolPrefix = "https://";
        private static readonly HashSet<string> Tlds = new(1577);

        public override PluginManifest Manifest { get; } = new("Url Launcher", PluginPriority.High)
        {
            Description = "Launch URLs.\n\nExamples: \"google.com\", \"images.google.com\""
        };

        public override async Task<bool> Initialize()
        {
            var asm = GetType().Assembly;
            await using var stream = asm.GetManifestResourceStream("Pinpoint.Plugin.UrlLauncher.tlds.txt");
            using var reader = new StreamReader(stream, Encoding.UTF8);
            while (await reader.ReadLineAsync() is { } line)
            {
                Tlds.Add(line);
            }

            return true;
        }

        public void Unload() { }

        public override async Task<bool> ShouldActivate(Query query)
        {
            if (query.Raw.Length < 3 || query.Raw[0] == '.' || !query.Raw.Contains("."))
            {
                return false;
            }

            var split = query.Raw.Split(".");
            return split.Length > 0 && Tlds.Contains(split[^1]);
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var url = query.Raw;
            if (!query.Raw.StartsWith(HttpsProtocolPrefix))
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
