using System.Collections.Generic;

namespace Pinpoint.Plugin.Bookmarks
{
    public interface IBookmarkExtractor
    {
        IEnumerable<AbstractBookmarkModel> Extract();
    }
}