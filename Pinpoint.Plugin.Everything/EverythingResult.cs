using System.Diagnostics;
using System.IO;
using FontAwesome5;
using Pinpoint.Plugin.Everything.API;

namespace Pinpoint.Plugin.Everything
{
    public class EverythingResult : AbstractQueryResult
    {
        private readonly QueryResultItem _item;

        public EverythingResult(QueryResultItem item, EFontAwesomeIcon icon) : base(
            item.ResultType != ResultType.Directory ? item.Name : Path.GetFileName(item.FullPath),
            item.FullPath
            )
        {
            _item = item;
            Icon = icon;
        }

        public override EFontAwesomeIcon Icon { get; }

        public override void OnSelect()
        {
            Process.Start("explorer.exe", _item.FullPath);
        }
    }
}