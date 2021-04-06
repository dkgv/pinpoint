using System;

namespace Pinpoint.Plugin.ClipboardManager
{
    public interface IClipboardEntry
    {
        public string Title { get; set; }

        public object Content { get; set; }

        public DateTime Copied { get; set; }
    }
}
