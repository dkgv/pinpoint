using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FontAwesome5;
using FontAwesome5.Extensions;
using Svg;

namespace Pinpoint.Core.Results;

public static class FontAwesomeBitmapRepository
{
    private static readonly Dictionary<EFontAwesomeIcon, Bitmap> IconCache = new();

    public static Bitmap Get(EFontAwesomeIcon icon)
    {
        // Check if we already converted FA icon -> Bitmap and if not, do it
        if (!IconCache.ContainsKey(icon))
        {
            if (icon.GetSvg(out string path, out int width, out int height))
            {
                var xml = CreateXmlDocument(width, height, path);
                var document = SvgDocument.FromSvg<SvgDocument>(xml);
                IconCache[icon] = document.Draw();
            }
        }

        return IconCache.TryGetValue(icon, out Bitmap value) ? value : default;
    }

    private static string CreateXmlDocument(int width, int height, string path)
    {
        var sb = new StringBuilder();
        sb.Append($"<svg width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\" xmlns=\"http://www.w3.org/2000/svg\">");
        sb.Append($"<path d=\"{path}\"/>");
        sb.Append("</svg>");
        return sb.ToString();
    }
}