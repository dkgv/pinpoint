using System.Diagnostics;
using System.Drawing;
using System.IO;
using FontAwesome5;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Everything.API;

namespace Pinpoint.Plugin.Everything
{
    public class EverythingResult : AbstractQueryResult
    {
        private readonly QueryResultItem _item;

        public EverythingResult(QueryResultItem item) : base(
            item.ResultType != ResultType.Directory ? item.Name : Path.GetFileName(item.FullPath),
            item.FullPath
            )
        {
            _item = item;
            
            Options.Add(new RunAsAdminOption(item.FullPath));

            if (item.ResultType == ResultType.File)
            {
                Options.Add(new OpenLocationOption(item.FullPath));
            }
        }

        public override Bitmap Icon => _item.Icon;
        

        public override void OnSelect()
        {
            Process.Start("explorer.exe", _item.FullPath);
        }

        public override bool OnPrimaryOptionSelect()
        {
            Process.Start("explorer.exe", Path.GetDirectoryName(_item.FullPath));
            return true;
        }
    }
}