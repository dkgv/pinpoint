using System.Diagnostics;
using System.IO;
using FontAwesome5;
using Pinpoint.Plugin.Everything.API;

namespace Pinpoint.Plugin.Everything
{
    public class EverythingResult : AbstractFontAwesomeQueryResult
    {
        private readonly QueryResultItem _item;

        public EverythingResult(QueryResultItem item, EFontAwesomeIcon icon) : base(
            item.ResultType != ResultType.Directory ? item.Name : Path.GetFileName(item.FullPath),
            item.FullPath
            )
        {
            _item = item;
            FontAwesomeIcon = icon;
        }

        public override EFontAwesomeIcon FontAwesomeIcon { get; }

        public override void OnSelect()
        {
            Process.Start("explorer.exe", _item.FullPath);
        }
    }
}