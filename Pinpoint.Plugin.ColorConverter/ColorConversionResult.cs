using System.Drawing;
using System.Text;
using Pinpoint.Core.Results;
using Svg;

namespace Pinpoint.Plugin.ColorConverter
{
    public class ColorConversionResult : CopyabableQueryOption
    {
        private static string _iconColor = "";
        public ColorConversionResult(string title, string content) : base(title, "")
        {
            _iconColor = content;
        }
        
        public override Bitmap Icon
        {
            get
            {
                return SvgDocument.FromSvg<SvgDocument>(CreateIconFromXML()).Draw();
            }
        }
        
        private string CreateIconFromXML()
        {
            var sb = new StringBuilder();
            sb.Append($"<svg width=\"512\" height=\"512\" viewBox=\"0 0 512 512\" xmlns=\"http://www.w3.org/2000/svg\">");
            sb.Append($"<circle cx=\"256\" cy=\"256\" r=\"256\" fill=\"{_iconColor}\" />");
            sb.Append("</svg>");
            return sb.ToString();
        }
    }
}