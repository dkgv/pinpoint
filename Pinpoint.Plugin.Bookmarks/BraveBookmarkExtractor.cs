namespace Pinpoint.Plugin.Bookmarks
{
    public class BraveBookmarkExtractor : ChromeBookmarkExtractor
    {
        public BraveBookmarkExtractor(IBookmarkFileLocator locator): base(locator) { }
    }
}