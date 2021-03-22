using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Dictionary
{
    public class DictionaryPlugin : IPlugin
    {
        private static readonly Regex DefineRegex = new Regex(@"(meaning|def|define|definition)$");

        public PluginMeta Meta { get; set; } = new PluginMeta("Dictionary", PluginPriority.Standard);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool TryLoad() => true;

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return query.Parts.Length == 2 && DefineRegex.IsMatch(query.Parts[^1]);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var url = $"https://api.dictionaryapi.dev/api/v2/entries/en_US/{query.Parts[0]}";

            var httpResponse = await SendGet(url);
            if (string.IsNullOrEmpty(httpResponse))
            {
                yield break;
            }

            var matches = JArray.Parse(httpResponse);

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

        private async Task<string> SendGet(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                return await httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
