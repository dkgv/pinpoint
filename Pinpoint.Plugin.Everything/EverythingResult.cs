using System.Diagnostics;
using FontAwesome5;
using Pinpoint.Plugin.Everything.API;

namespace Pinpoint.Plugin.Everything
{
    public class EverythingResult : IQueryResult
    {
        private readonly QueryResultItem _item;

        public EverythingResult(QueryResultItem item, EFontAwesomeIcon icon)
        {
            _item = item;
            Title = item.Name;
            Subtitle = item.FullPath;
            Icon = icon;
        }

        public string Title { get; }

        public string Subtitle { get; }

        public object Instance { get; }

        public EFontAwesomeIcon Icon { get; }

        public void OnSelect()
        {
            if (_item.ResultType == ResultType.Directory)
            {
                Process.Start("explorer.exe", _item.FullPath);
            }
            else
            {
                Process.Start(_item.FullPath);
            }
        }
    }
}