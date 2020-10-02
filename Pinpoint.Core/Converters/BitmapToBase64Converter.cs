using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Pinpoint.Core.Converters
{
    public static class BitmapToBase64Converter
    {
        public static string Convert(Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            return System.Convert.ToBase64String(stream.ToArray());
        }

        public static Bitmap ConvertBack(string base64)
        {
            var bytes = System.Convert.FromBase64String(base64);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(Image.FromStream(stream));
        }
    }
}
