using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Clipboard;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.ClipboardUploader
{
    public class ClipboardUploaderPlugin : IPlugin
    {
        private const string Description = "Upload your clipboard content to a pastebin and receive the resulting URL directly to your clipboard.\n\nExamples: \"paste\"";

        public PluginMeta Meta { get; set; } = new PluginMeta("Clipboard Uploader", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public ClipboardHistory ClipboardHistory = new ClipboardHistory(1);

        public async Task<bool> Activate(Query query) => query.RawQuery.ToLower().Equals("paste");

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            if (ClipboardHistory.First == null)
            {
                yield break;
            }

            var entry = ClipboardHistory.First.Value as TextClipboardEntry;
            yield return new UploadToResult(new PastersUploader(), entry.Content);
        }
    }

    public class UploadToResult : UrlQueryResult
    {
        private readonly AbstractUploader _abstractUploader;
        private readonly string _content;

        public UploadToResult(AbstractUploader abstractUploader, string content) : base($"Paste clipboard content to {abstractUploader.Name}", "")
        {
            _abstractUploader = abstractUploader;
            _content = content;
        }

        public override void OnSelect()
        {
            Task.Run(async () =>
            {
                var result = await _abstractUploader.Upload(_content);
                if (string.IsNullOrEmpty(result))
                {
                    return;
                }

                ClipboardHelper.Copy(result.Trim());

                Url = result;
                base.OnSelect();
            });
        }

        public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_Paste;
    }
}
