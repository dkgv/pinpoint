using System.Drawing;
using System.Text;
using GEmojiSharp;
using Pinpoint.Core;
using Pinpoint.Core.Clipboard;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Emoji;

public class EmojiPlugin : IPlugin
{
    public PluginManifest Manifest { get; set; } = new("Emoji", PluginPriority.Highest)
    {
        Description = "Search for and insert emojis anywhere.\n\nExamples: \":smilin\""
    };

    public PluginStorage Storage { get; set; } = new();

    public async Task<bool> Activate(Query query) => query.RawQuery.Length > 1 && query.Prefix() == ":";

    public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, CancellationToken ct)
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
            ClipboardHelper.Copy(_emoji.Raw, Encoding.Unicode);
            ClipboardHelper.PasteClipboard();
        }
    }
}