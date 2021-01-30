using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Dictionary
{
    public class DictionaryPlugin : IPlugin
    {
        private static readonly BrowsingContext Ctx = new BrowsingContext(Configuration.Default.WithDefaultLoader());
        private const string ContainerSelector = "section.css-pnw38j:nth-child(2) > div:nth-child(2)";
        private static readonly Regex DefineRegex = new Regex(@"(meaning|def|define|definition)$");

        public PluginMeta Meta { get; set; } = new PluginMeta("Dictionary", PluginPriority.Standard);

        public void Load()
        {
        }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return query.Parts.Length == 2 && DefineRegex.IsMatch(query.Parts[^1]);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var url = $"https://www.dictionary.com/browse/{query.Parts[0]}";

            var document = await Ctx.OpenAsync(url);
            var container = document.Body.QuerySelector(ContainerSelector);
            if (container == null)
            {
                yield break;
            }

            // Grab at most first five definitions
            var definitions = container.QuerySelectorAll("div");
            for (var i = 0; i < Math.Min(definitions.Length, 5); i++)
            {
                var text = definitions[i].TextContent;
                if (text.Contains(":"))
                {
                    text = text.Split(":")[0];
                }

                yield return new DictionaryResult(text, url);
            }
        }
    }
}
