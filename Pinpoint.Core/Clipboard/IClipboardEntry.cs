using System;

namespace Pinpoint.Core.Clipboard
{
    public interface IClipboardEntry
    {
        public string Title { get; set; }

        public DateTime Copied { get; set; }
    }
}
