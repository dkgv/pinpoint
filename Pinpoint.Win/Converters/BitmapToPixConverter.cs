using System;
using System.Drawing;
using System.Drawing.Imaging;
using Tesseract;

namespace Pinpoint.Win.Converters
{
    public static class BitmapToPixConverter
    {
        public static Pix ToPix(Bitmap img)
        {
            int pixDepth = GetPixDepth(img.PixelFormat);
            Pix pix = Pix.Create(img.Width, img.Height, pixDepth);
            pix.XRes = (int)Math.Round(img.HorizontalResolution);
            pix.YRes = (int)Math.Round(img.VerticalResolution);
            BitmapData bitmapData = null;
            try
            {
                if ((img.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
                {
                    CopyColormap(img, pix);
                }
                bitmapData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
                PixData data = pix.GetData();
                if (bitmapData.PixelFormat == PixelFormat.Format32bppArgb)
                    TransferDataFormat32bppArgb(bitmapData, data);
                else if (bitmapData.PixelFormat == PixelFormat.Format32bppRgb)
                    TransferDataFormat32bppRgb(bitmapData, data);
                else if (bitmapData.PixelFormat == PixelFormat.Format24bppRgb)
                    TransferDataFormat24bppRgb(bitmapData, data);
                else if (bitmapData.PixelFormat == PixelFormat.Format8bppIndexed)
                    TransferDataFormat8bppIndexed(bitmapData, data);
                else if (bitmapData.PixelFormat == PixelFormat.Format1bppIndexed)
                    TransferDataFormat1bppIndexed(bitmapData, data);
                return pix;
            }
            catch (Exception)
            {
                pix.Dispose();
                throw;
            }
            finally
            {
                if (bitmapData != null)
                    img.UnlockBits(bitmapData);
            }
        }

        private static void CopyColormap(Bitmap img, Pix pix)
        {
            Color[] entries = img.Palette.Entries;
            PixColormap pixColormap = PixColormap.Create(pix.Depth);
            try
            {
                for (int index = 0; index < entries.Length; ++index)
                {
                    if (!pixColormap.AddColor(entries[index].ToPixColor()))
                        throw new InvalidOperationException(string.Format("Failed to add colormap entry {0}.", (object)index));
                }
                pix.Colormap = pixColormap;
            }
            catch (Exception)
            {
                pixColormap.Dispose();
                throw;
            }
        }

        public static PixColor ToPixColor(this Color color)
        {
            return new PixColor(color.R, color.G, color.B, color.A);
        }

        private static int GetPixDepth(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                    return 32;
                case PixelFormat.Format1bppIndexed:
                    return 1;
                case PixelFormat.Format8bppIndexed:
                    return 8;
                default:
                    throw new InvalidOperationException(string.Format("Source bitmap's pixel format {0} is not supported.", (object)pixelFormat));
            }
        }

        private static unsafe void TransferDataFormat1bppIndexed(BitmapData imgData, PixData pixData)
        {
            int height = imgData.Height;
            int num = imgData.Width / 8;
            for (int index1 = 0; index1 < height; ++index1)
            {
                byte* data1 = (byte*)((IntPtr)(void*)imgData.Scan0 + index1 * imgData.Stride);
                uint* data2 = (uint*)((IntPtr)(void*)pixData.Data + (index1 * pixData.WordsPerLine) * 4);
                for (int index2 = 0; index2 < num; ++index2)
                {
                    byte dataByte = BitmapHelper.GetDataByte(data1, index2);
                    PixData.SetDataByte(data2, index2, (uint)dataByte);
                }
            }
        }

        private static unsafe void TransferDataFormat24bppRgb(BitmapData imgData, PixData pixData)
        {
            int pixelFormat = (int)imgData.PixelFormat;
            int height = imgData.Height;
            int width = imgData.Width;
            for (int index1 = 0; index1 < height; ++index1)
            {
                byte* numPtr1 = (byte*)((IntPtr)(void*)imgData.Scan0 + index1 * imgData.Stride);
                uint* data = (uint*)((IntPtr)(void*)pixData.Data + (index1 * pixData.WordsPerLine) * 4);
                for (int index2 = 0; index2 < width; ++index2)
                {
                    byte* numPtr2 = numPtr1 + index2 * 3;
                    byte blue = *numPtr2;
                    byte green = numPtr2[1];
                    byte red = numPtr2[2];
                    PixData.SetDataFourByte(data, index2, BitmapHelper.EncodeAsRGBA(red, green, blue, byte.MaxValue));
                }
            }
        }

        private static unsafe void TransferDataFormat32bppRgb(BitmapData imgData, PixData pixData)
        {
            int pixelFormat = (int)imgData.PixelFormat;
            int height = imgData.Height;
            int width = imgData.Width;
            for (int index1 = 0; index1 < height; ++index1)
            {
                byte* numPtr1 = (byte*)((IntPtr)(void*)imgData.Scan0 + index1 * imgData.Stride);
                uint* data = (uint*)((IntPtr)(void*)pixData.Data + (index1 * pixData.WordsPerLine) * 4);
                for (int index2 = 0; index2 < width; ++index2)
                {
                    byte* numPtr2 = numPtr1 + (index2 << 2);
                    byte blue = *numPtr2;
                    byte green = numPtr2[1];
                    byte red = numPtr2[2];
                    PixData.SetDataFourByte(data, index2, BitmapHelper.EncodeAsRGBA(red, green, blue, byte.MaxValue));
                }
            }
        }

        private static unsafe void TransferDataFormat32bppArgb(BitmapData imgData, PixData pixData)
        {
            int pixelFormat = (int)imgData.PixelFormat;
            int height = imgData.Height;
            int width = imgData.Width;
            for (int index1 = 0; index1 < height; ++index1)
            {
                byte* numPtr1 = (byte*)((IntPtr)(void*)imgData.Scan0 + index1 * imgData.Stride);
                uint* data = (uint*)((IntPtr)(void*)pixData.Data + (index1 * pixData.WordsPerLine) * 4);
                for (int index2 = 0; index2 < width; ++index2)
                {
                    byte* numPtr2 = numPtr1 + (index2 << 2);
                    byte blue = *numPtr2;
                    byte green = numPtr2[1];
                    byte red = numPtr2[2];
                    byte alpha = numPtr2[3];
                    PixData.SetDataFourByte(data, index2, BitmapHelper.EncodeAsRGBA(red, green, blue, alpha));
                }
            }
        }

        private static unsafe void TransferDataFormat8bppIndexed(BitmapData imgData, PixData pixData)
        {
            int height = imgData.Height;
            int width = imgData.Width;
            for (int index1 = 0; index1 < height; ++index1)
            {
                byte* numPtr = (byte*)((IntPtr)(void*)imgData.Scan0 + index1 * imgData.Stride);
                uint* data = (uint*)((IntPtr)(void*)pixData.Data + (index1 * pixData.WordsPerLine) * 4);
                for (int index2 = 0; index2 < width; ++index2)
                {
                    byte num = numPtr[index2];
                    PixData.SetDataByte(data, index2, (uint)num);
                }
            }
        }
    }
}
