using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Pinpoint.Core.Converters
{
    public static class BitmapBase64Converter
    {
        public static Bitmap FromBase64(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(Image.FromStream(stream));
        }

        public static string ToBase64(Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            return Convert.ToBase64String(stream.ToArray());
        }
    }
}
