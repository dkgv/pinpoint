using System.Drawing;
using System.Globalization;
using System.Text;
using GEmojiSharp;
using Pinpoint.Core;
using Pinpoint.Core.Clipboard;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Emoji;

public class EmojiPlugin : AbstractPlugin
{
    public override PluginManifest Manifest { get; } = new("Emoji", PluginPriority.High)
    {
        Description = "Search for and insert emojis anywhere.\n\nExamples: \":smilin\""
    };

    public override async Task<bool> ShouldActivate(Query query) => query.RawQuery.Length > 1 && query.Prefix() == ":";

    public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, CancellationToken ct)
    {
        foreach (var emoji in GEmojiSharp.Emoji.Find(query.RawQuery))
        {
            yield return new EmojiResult(emoji);
        }
    }

    private class EmojiResult : AbstractQueryResult
    {
        private readonly GEmoji _emoji;

        public EmojiResult(GEmoji emoji) : base(emoji.Raw + " " + emoji.Description, string.Join(",", emoji.Aliases))
        {
            _emoji = emoji;
        }

        public override Bitmap Icon { get; } = new(1, 1);

        public override void OnSelect()
        {
            var unicode = _emoji.Raw;
            ClipboardHelper.Copy(unicode);
            ClipboardHelper.PasteClipboard();
        }
    }
}