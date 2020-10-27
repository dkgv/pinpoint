using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FontAwesome5;
using FontAwesome5.Extensions;
using Svg;

namespace Pinpoint.Plugin
{
    public abstract class AbstractFontAwesomeQueryResult : AbstractQueryResult
    {
        private static readonly Dictionary<EFontAwesomeIcon, Bitmap> IconCache = new Dictionary<EFontAwesomeIcon, Bitmap>();

        protected AbstractFontAwesomeQueryResult()
        {
        }

        protected AbstractFontAwesomeQueryResult(string title, string subtitle = "") : base(title, subtitle)
        {
        }

        public abstract EFontAwesomeIcon FontAwesomeIcon { get; }

        public override Bitmap Icon
        {
            get
            {
                // Check if we already converted FA icon -> Bitmap and if not, do it
                if (!IconCache.ContainsKey(FontAwesomeIcon))
                {
                    var attr = FontAwesomeIcon.GetInformationAttribute<FontAwesomeSvgInformationAttribute>();
                    var xml = CreateXMLDocument(attr.Width, attr.Height, attr.Path);
                    var document = SvgDocument.FromSvg<SvgDocument>(xml);
                    IconCache[FontAwesomeIcon] = document.Draw();
                }

                return IconCache[FontAwesomeIcon];
            }
        }

        private string CreateXMLDocument(int width, int height, string path)
        {
            var sb = new StringBuilder();
            sb.Append($"<svg width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\" xmlns=\"http://www.w3.org/2000/svg\">");
            sb.Append($"<path d=\"{path}\"/>");
            sb.Append("</svg>");
            return sb.ToString();
        }
    }
}