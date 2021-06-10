using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Text
{
    public class TextPlugin : IPlugin
    {
        private const string Description = "Easily perform various transformative text actions. Wrap your text in \" and select and option.\n\nExamples: \"this is some text\"";

        public PluginMeta Meta { get; set; } = new PluginMeta("Text Plugin", Description, PluginPriority.NextHighest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public async Task<bool> Activate(Query query) => query.RawQuery.Length > 2 && query.RawQuery.StartsWith("\"") && query.RawQuery.EndsWith("\"");

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var text = query.RawQuery[1..^1];
            yield return new URLOption(HttpUtility.UrlEncode(text), true);
            yield return new URLOption(HttpUtility.UrlDecode(text), false);
            yield return new CaseOption(text.ToUpper(), true);
            yield return new CaseOption(text.ToLower(), false);
            yield return new ReverseOption(new string(text.Reverse().ToArray()));
            yield return new UniqueOption(new string(text.ToCharArray().Distinct().ToArray()));
        }

        private class ReverseOption : CopyabableQueryOption
        {
            public ReverseOption(string content) : base($"Reversed: {content}", content)
            {
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_Sync;
        }

        private class CaseOption : CopyabableQueryOption
        {
            public CaseOption(string content, bool upper) : base($"{(upper ? "Upper" : "Lower")}case: {content}", content)
            {
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_Font;
        }

        private class UniqueOption : CopyabableQueryOption
        {
            public UniqueOption(string content) : base($"Distinct characters: {content}", content)
            {
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_Fingerprint;
        }

        private class URLOption : CopyabableQueryOption
        {
            public URLOption(string content, bool encode) : base($"URL {(encode ? "encod" : "decod")}ed: {content}", content)
            {
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_Code;
        }
    }
}
