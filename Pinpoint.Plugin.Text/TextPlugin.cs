﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Text
{
    public class TextPlugin : AbstractPlugin
    {
        public override PluginManifest Manifest { get; } = new("Text Plugin")
        {
            Description = "Easily perform various transformative text actions. Wrap your text in \" and select and option.\n\nExamples: \"this is some text\""
        };

        public override async Task<bool> ShouldActivate(Query query) => query.Raw.Length > 2 && query.Raw.StartsWith("\"") && query.Raw.EndsWith("\"");

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var text = query.Raw[1..^1];
            yield return new URLOption(HttpUtility.UrlEncode(text), true);
            yield return new URLOption(HttpUtility.UrlDecode(text), false);

            if (text.Any(char.IsLower))
            {
                yield return new CaseOption(text.ToUpper(), true);
            }

            if (text.Any(char.IsUpper))
            {
                yield return new CaseOption(text.ToLower(), false);
            }

            yield return new ReverseOption(new string(text.Reverse().ToArray()));

            var distinct = new string(text.ToCharArray().Distinct().ToArray());
            if (!text.Equals(distinct))
            {
                yield return new UniqueOption(distinct);
            }

            if (text.Contains(" "))
            {
                yield return new NoSpacesOption(text.Replace(" ", ""));
            }

            yield return new CharCountOption(text.Length.ToString());
        }

        private class CharCountOption : CopyabableQueryOption
        {
            public CharCountOption(string content) : base($"Char count: {content}", content)
            {
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_RulerHorizontal;
        }

        private class NoSpacesOption : CopyabableQueryOption
        {
            public NoSpacesOption(string content) : base($"No spaces: {content}", content)
            {
            }

            public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_Backspace;
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
