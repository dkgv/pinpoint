using System;
using System.Diagnostics;
using System.Drawing;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Shortcuts
{
    public class ShortcutsResult : AbstractQueryResult
    {
        private readonly string _shortcut;

        public ShortcutsResult(string name, string shortcut) : base(name, shortcut)
        {
            _shortcut = shortcut;
        }

        public override Bitmap Icon { get; } = FontAwesomeBitmapRepository.Get(EFontAwesomeIcon.Solid_Cog);

        public override void OnSelect()
        {
            var isUrl = Uri.TryCreate(_shortcut, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isUrl)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = uriResult.AbsoluteUri,
                    UseShellExecute = true
                });
                return;
            }

            Process.Start("explorer.exe", _shortcut);
        }
    }
}