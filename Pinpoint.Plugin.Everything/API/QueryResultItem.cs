using System.Drawing;

namespace Pinpoint.Plugin.Everything.API
{
    public class QueryResultItem
    {
        public ResultType ResultType { get; set; }

        public string Name { get; set; }

        public string FullPath { get; set; }

        public Bitmap Icon { get; set; }
    }
}