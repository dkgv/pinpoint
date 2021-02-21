namespace Pinpoint.Plugin.Bookmarks
{
    public abstract class AbstractBookmarkModel
    {
        public abstract string Name { get; set; }

        public abstract string Url { get; set; }
    }
}