using System;
using System.Drawing;

namespace Pinpoint.Core.Clipboard
{
    public class TextClipboardEntry : IClipboardEntry
    {
        public TextClipboardEntry()
        {
            Copied = DateTime.Now;
        }

        public string Title { get; set; }

        public string Content { get; set; }
        
        public DateTime Copied { get; set; }

        public Bitmap Icon { get; set; }
    }
}
