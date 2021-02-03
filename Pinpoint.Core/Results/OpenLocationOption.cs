using System.Diagnostics;
using System.IO;
using FontAwesome5;

namespace Pinpoint.Core.Results
{
    public class OpenLocationOption : AbstractFontAwesomeQueryResult
    {
        private readonly string _filePath;

        public OpenLocationOption(string filePath) : base("Open File Location", Path.GetDirectoryName(filePath))
        {
            _filePath = Path.GetDirectoryName(filePath);
        }

        public override void OnSelect()
        {
            Process.Start("explorer.exe", "\"" + _filePath + "\"");
        }

        public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Regular_FolderOpen;
    }
}