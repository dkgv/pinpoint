using System;

namespace Pinpoint.Plugin.ClipboardManager
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
    }
}
