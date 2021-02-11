using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Bookmarks
{
    public class FirefoxBookmarkResult : UrlQueryResult
    {
        public FirefoxBookmarkResult(string url) : base(url)
        {
        }

        public FirefoxBookmarkResult(string title, string url) : base(title, url)
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Brands_FirefoxBrowser;
    }
}