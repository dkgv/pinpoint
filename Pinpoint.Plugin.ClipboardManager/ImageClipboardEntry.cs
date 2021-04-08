using System;
using System.Drawing;

namespace Pinpoint.Plugin.ClipboardManager
{
    public class ImageClipboardEntry : IClipboardEntry
    {
        public ImageClipboardEntry()
        {
            Copied = DateTime.Now;
        }
        public string Title { get; set; }
        
        public Image Content { get; set; }
        
        public DateTime Copied { get; set; }
    }
}