using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.UrlLauncher
{
    public class UrlLauncherPlugin: IPlugin
    {
        private const string HttpsProtocolPrefix = "https://";
        private readonly Regex _urlRegex = new Regex(@"[0-9a-zA-Z]+\.[0-9a-zA-Z]+[\/\?\%\=\&0-9a-zA-Z]+$");

        public PluginMeta Meta { get; set; } = new PluginMeta("Url launcher", PluginPriority.Highest);
        public bool TryLoad() => true;

        public void Unload() { }

        public Task<bool> Activate(Query query)
        {
            var isUrl = _urlRegex.IsMatch(query.RawQuery);
            return Task.FromResult(isUrl);
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
