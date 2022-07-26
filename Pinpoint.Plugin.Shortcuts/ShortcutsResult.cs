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
            Process.Start("explorer.exe", _shortcut);
        }
    }
}