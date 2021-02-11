using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Bookmarks
{
    public class ChromeBookmarkResult : UrlQueryResult
    {
        public ChromeBookmarkResult(string url) : base(url)
        {
        }

        public ChromeBookmarkResult(string title, string url) : base(title, url)
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Brands_Chrome;
    }
}
