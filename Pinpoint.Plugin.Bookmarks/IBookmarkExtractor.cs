using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Bookmarks
{
    public interface IBookmarkExtractor
    {
        Task<IEnumerable<AbstractBookmarkModel>> Extract();
    }
}