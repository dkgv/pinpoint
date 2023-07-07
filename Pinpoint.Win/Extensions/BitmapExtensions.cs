using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Pinpoint.Win.Extensions
{
    public static class BitmapExtensions
    {
        public static ImageSource ToImageSource(this Bitmap bitmap)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            try
            {
                var size = rect.Width * rect.Height * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        public static Bitmap ToBlackAndWhite(this Bitmap bitmap)
        {
            unsafe Bitmap Convert(byte[] bytes, BitmapData data)
            {
                var p = (byte*)data.Scan0;
                for (var i = 0; i < bytes.Length; i += 4)
                {
                    byte r = bytes[i + 2], g = bytes[i + 1], b = bytes[i];
                    var avg = (byte)((r + g + b) / 3);
                    p[i + 2] = p[i + 1] = p[i] = avg;
                }
                return bitmap;
            }

            return FastBitmapOpDispatcher(bitmap, Convert);
        }

        public static Bitmap InvertColors(this Bitmap bitmap)
        {
            unsafe Bitmap Invert(byte[] bytes, BitmapData data)
            {
                byte* p = (byte*)data.Scan0;
                for (var i = 0; i < bytes.Length; i += 4)
                {
                    byte r = bytes[i + 2], g = bytes[i + 1], b = bytes[i];
                    p[i + 2] = (byte)(255 - r);
                    p[i + 1] = (byte)(255 - g);
                    p[i] = (byte)(255 - b);
                }
                return bitmap;
            }

            return FastBitmapOpDispatcher(bitmap, Invert);
        }

        private static unsafe T FastBitmapOpDispatcher<T>(Bitmap bitmap, Func<byte[], BitmapData, T> op)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var bytes = new byte[data.Height * data.Stride];
            var pointer = (byte*)data.Scan0;
            Marshal.Copy((IntPtr)pointer, bytes, 0, data.Height * data.Stride);
            var t = op(bytes, data);
            bitmap.UnlockBits(data);
            return t;
        }
    }
}
