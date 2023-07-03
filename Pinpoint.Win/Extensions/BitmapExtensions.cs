using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pinpoint.Win.Extensions
{
    public static class BitmapExtensions
    {
        public static ImageSource ToImageSource(this Bitmap bitmap)
        {
            unsafe BitmapSource Convert(byte[] bytes, BitmapData data)
            {
                var pixelFormat = data.PixelFormat switch
                {
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb => System.Windows.Media.PixelFormats.Pbgra32,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb => System.Windows.Media.PixelFormats.Bgr24,
                    System.Drawing.Imaging.PixelFormat.Format32bppRgb => System.Windows.Media.PixelFormats.Bgr32,
                    System.Drawing.Imaging.PixelFormat.Format16bppRgb555 => System.Windows.Media.PixelFormats.Bgr555,
                    System.Drawing.Imaging.PixelFormat.Format16bppRgb565 => System.Windows.Media.PixelFormats.Bgr565,
                    _ => throw new ArgumentException($"Unknown pixel format: {data.PixelFormat}")
                };

                return BitmapSource.Create(
                    data.Width,
                    data.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    pixelFormat,
                    null,
                    bytes,
                    data.Stride
                );
            }

            return FastBitmapOpDispatcher(bitmap, Convert);
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
