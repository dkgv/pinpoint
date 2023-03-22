using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Dictionary
{
    public class DictionaryPlugin : IPlugin
    {
        private const string Description = "Find word definitions.\n\nExamples: \"define ubiquitous\"";

        private static readonly Regex DefineRegex = new(@"^(define)");

        public PluginMeta Meta { get; set; } = new("Dictionary", Description, PluginPriority.Standard);

        public PluginStorage Storage { get; set; } = new();

        public async Task<bool> Activate(Query query)
        {
            return query.Parts.Length == 2 && DefineRegex.IsMatch(query.Parts[0]);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var url = $"https://api.dictionaryapi.dev/api/v2/entries/en_US/{query.Parts[^1]}";

            var matches = await HttpHelper.SendGet(url, JArray.Parse);

            if (matches == null || matches.Count == 0)
            {
                yield break;
            }

            var meanings = matches[0]["meanings"].ToArray();
            foreach (var meaning in meanings)
            {
                var definitions = meaning["definitions"].ToArray();
                for (var i = 0; i < Math.Min(definitions.Length, 2); i++)
                {
                    var model = JsonConvert.DeserializeObject<DefinitionModel>(definitions[i].ToString());
                    model.PartOfSpeech = meaning["partOfSpeech"].ToString();
                    yield return new DictionaryResult(model);
                }
            }
        }
    }
}
