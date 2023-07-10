using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FontAwesome5;
using Newtonsoft.Json;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Translate
{
    public class TranslatePlugin : AbstractPlugin
    {
        public override PluginManifest Manifest { get; } = new("Translate Plugin")
        {
            Description = "Translate to and from different languages.\n\nExamples: \"fr,en bonjour\", \"<from>,<to> <text>\""
        };

        public override PluginState State { get; } = new()
        {
            DebounceTime = TimeSpan.FromMilliseconds(250),
        };

        public override async Task<bool> ShouldActivate(Query query)
        {
            if (query.Parts.Length < 2)
            {
                return false;
            }

            var first = query.Parts[0];
            // fr,en
            if (first.Length != 5 || !first.Contains(","))
            {
                return false;
            }

            return true;
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var first = query.Parts[0];
            var parts = first.Split(",");
            var from = parts[0];
            var to = parts[1];
            var content = string.Join(" ", query.Parts[1..]);

            var translation = await Translate(from, to, content);
            if (translation == null)
            {
                yield break;
            }

            yield return new TranslationResult(translation.Translation.ToLower());
        }

        private async Task<TranslationModel> Translate(string from, string to, string content)
        {
            var bodyContent = JsonConvert.SerializeObject(new Dictionary<string, string> { { "content", content } });
            var body = new StringContent(bodyContent, Encoding.UTF8, "application/json");

            var url = $"https://usepinpoint.com/api/translate/{from}/{to}";
            var response = await HttpHelper.SendPost(url, body, async message =>
            {
                var raw = await message.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TranslationModel>(raw);
            });
            if (response == null)
            {
                return null;
            }

            return await response;
        }

        record TranslationModel([JsonProperty("translation")] string Translation);

        class TranslationResult : CopyabableQueryOption
        {
            public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Language;

            public TranslationResult(string title, string content) : base(title, content)
            {
            }

            public TranslationResult(string content) : base(content)
            {
            }
        }
    }
}
