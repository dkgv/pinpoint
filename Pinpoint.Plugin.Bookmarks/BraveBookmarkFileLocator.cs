using System;
using System.IO;

namespace Pinpoint.Plugin.Bookmarks
{
    public class BraveBookmarkFileLocator: IBookmarkFileLocator
    {        
        public string Locate()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BraveSoftware",
                "BraveBrowser",
                "User Data",
                "Default",
                "Bookmarks"
            );

            return path;
        }
    }
}