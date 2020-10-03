using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Pinpoint.Win.Converters;
using Tesseract;

namespace Pinpoint.Win.Extensions
{
    public static class BitmapExtensions
    {
        private static readonly TesseractEngine OcrEngine = new TesseractEngine("./tessdata", "eng", EngineMode.Default);

        public static Bitmap Crop(this Bitmap bitmap, Rectangle region)
        {
            if (bitmap == null || bitmap.Width == 0 || bitmap.Height == 0)
            {
                return default;
            }

            if (region.Width == 0 || region.Height == 0)
            {
                return bitmap;
            }
            
            var tmp = new Bitmap(bitmap);
            var cropped = new Bitmap(region.Width, region.Height);

            using (var g = Graphics.FromImage(cropped))
            {
                g.DrawImage(tmp, -region.X, -region.Y);
            }

            tmp.Dispose();

            return cropped;
        }

        public static Tuple<string, Rectangle> Ocr(this Bitmap bitmap)
        {
            using var img = OcrEngine.Process(BitmapToPixConverter.ToPix(bitmap));
            var tessRect = img.RegionOfInterest;
            var rect = new Rectangle(tessRect.X1, tessRect.Y1, tessRect.Width, tessRect.Height);
            return new Tuple<string, Rectangle>(img.GetText().Trim().Replace('”', '"'), rect);
        }

        public static Bitmap Scale(this Bitmap bitmap, double scalar)
        {
            return new Bitmap(bitmap, new Size((int)(bitmap.Width * scalar), (int)(bitmap.Height * scalar)));
        }

        public static Bitmap CropToContent(this Bitmap bitmap, int padding = 5)
        {
            unsafe Rectangle ComputeContentArea(byte[] bytes, BitmapData data)
            {
                byte bgR = bytes[2], bgG = bytes[1], bgB = bytes[0];

                int minX = int.MaxValue, minY = int.MaxValue;
                int maxX = int.MinValue, maxY = int.MinValue;

                var p = (byte*)data.Scan0;
                var offset = data.Stride - bitmap.Width * 4;
                for (var y = 0; y < data.Height; y++)
                {
                    for (var x = 0; x < data.Width; x++)
                    {
                        byte r = p[0], g = p[1], b = p[2];
                        if (bgR != r && bgG != g && bgB != b)
                        {
                            minX = Math.Min(minX, x);
                            minY = Math.Min(minY, y);
                            maxX = Math.Max(maxX, x);
                            maxY = Math.Max(maxY, y);
                        }
                        p += 4;
                    }
                    p += offset;
                }

                minX = Math.Max(0, minX - padding);
                minY = Math.Max(0, minY - padding);
                maxX = Math.Min(data.Width, maxX + padding);
                maxY = Math.Min(data.Height, maxY + padding);
                
                return new Rectangle(minX, minY, maxX - minX, maxY - minY);
            }
            
            var contentArea = FastBitmapOpDispatcher(bitmap, ComputeContentArea);
            return Crop(bitmap, contentArea);
        }

        public static Bitmap ToBlackAndWhite(this Bitmap bitmap)
        {
            unsafe Bitmap Convert(byte[] bytes, BitmapData data)
            {
                var p = (byte*)data.Scan0;
                for (var i = 0; i < bytes.Length; i += 4)
                {
                    byte r = bytes[i + 2], g = bytes[i + 1], b = bytes[i];
                    var avg = (byte) ((r + g + b) / 3);
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
                byte* p = (byte*) data.Scan0;
                for (var i = 0; i < bytes.Length; i += 4)
                {
                    byte r = bytes[i + 2], g = bytes[i + 1], b = bytes[i];
                    p[i + 2] = (byte) (255 - r);
                    p[i + 1] = (byte) (255 - g);
                    p[i] = (byte) (255 - b);
                }
                return bitmap;
            }

            return FastBitmapOpDispatcher(bitmap, Invert);
        }

        public static bool IsDark(this Bitmap bitmap, double percent)
        {
            int Brightness(byte r, byte g, byte b)
            {
                return (int) Math.Sqrt(r * r * .241 + g * g * .691 + b * b * .068); // Returns between 0-255
            }

            bool Decider(byte[] bytes, BitmapData data)
            {
                int count = 0, all = bitmap.Width * bitmap.Height;
                for (var i = 0; i < bytes.Length; i += 4)
                {
                    byte r = bytes[i + 2], g = bytes[i + 1], b = bytes[i];
                    var brightness = Brightness(r, g, b);
                    if (brightness <= 127.5)
                    {
                        count++;
                    }
                }
                var ratio = 1d * count / (1d * all);
                return ratio * 100 >= percent;
            }

            return FastBitmapOpDispatcher(bitmap, Decider);
        }

        private static unsafe T FastBitmapOpDispatcher<T>(Bitmap bitmap, Func<byte[], BitmapData, T> op)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var bytes = new byte[data.Height * data.Stride];
            var pointer = (byte*)data.Scan0;
            Marshal.Copy((IntPtr) pointer, bytes, 0, data.Height * data.Stride);
            var t = op(bytes, data);
            bitmap.UnlockBits(data);
            return t;
        }
    }
}
