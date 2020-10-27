using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pinpoint.Win.Extensions
{
    public static class ImageSourceExtensions
    {
        public static Bitmap ToBitmap(this ImageSource source)
        {
            using var outStream = new MemoryStream();
            var bmpSource = source as BitmapSource;
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmpSource));
            enc.Save(outStream);
            var bitmap = new Bitmap(outStream);
            return new Bitmap(bitmap);
        }
    }
}