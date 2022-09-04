using System.Collections.Generic;

namespace Pinpoint.Plugin.Bookmarks
{
    public interface IBookmarkFileLocator
    {
        IEnumerable<string> Locate();
    }
}