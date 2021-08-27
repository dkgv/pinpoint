using System;
using System.Drawing;

namespace Pinpoint.Core.Clipboard
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