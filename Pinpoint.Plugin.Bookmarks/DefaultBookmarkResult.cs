using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Bookmarks
{
    public class DefaultBookmarkResult : UrlQueryResult
    {
        public DefaultBookmarkResult(string url) : base(url)
        {
        }

        public DefaultBookmarkResult(string title, string url) : base(title, url)
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Regular_Bookmark;
    }
}