﻿using System.Collections.Generic;

namespace Pinpoint.Plugin.ClipboardManager
{
    public class ClipboardHistory : LinkedList<IClipboardEntry>
    {
        public new void AddFirst(IClipboardEntry entry)
        {
            if (Contains(entry))
            {
                return;
            }

            if (Count > 10)
            {
                RemoveLast();
            }

            base.AddFirst(entry);
        }
    }
}
