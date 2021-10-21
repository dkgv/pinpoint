using System;
using System.Drawing;

namespace Pinpoint.Core.Clipboard
{
    public interface IClipboardEntry
    {
        public string Title { get; set; }

        public DateTime Copied { get; set; }

        public Bitmap Icon { get; set; }
    }
}
