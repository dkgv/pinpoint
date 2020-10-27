using System.Drawing;
using System.IO;

namespace Pinpoint.Win.Converters
{
    public static class Base64ToBitmapConverter
    {
        public static Bitmap Convert(string base64)
        {
            var bytes = System.Convert.FromBase64String(base64);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
    }
}