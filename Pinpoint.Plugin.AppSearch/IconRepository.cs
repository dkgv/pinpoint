using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Pinpoint.Plugin.AppSearch;

public static class IconRepository
{
    private static readonly Dictionary<string, Bitmap> IconCache = new();

    public static Bitmap GetIcon(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        if (!IconCache.ContainsKey(path))
        {
            if (path.Contains(".png"))
            {
                IconCache[path] = new Bitmap(path);
            }
            else
            {
                IconCache[path] = Icon.ExtractAssociatedIcon(path)?.ToBitmap();
            }
        }

        return IconCache[path];
    }
}