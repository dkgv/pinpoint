using System;
using System.Collections.Generic;
using System.IO;

namespace Pinpoint.Plugin.Bookmarks
{
    public class BraveBookmarkFileLocator: IBookmarkFileLocator
    {        
        public IEnumerable<string> Locate()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BraveSoftware",
                "BraveBrowser",
                "User Data",
                "Default",
                "Bookmarks"
            );

            yield return path;
        }
    }
}