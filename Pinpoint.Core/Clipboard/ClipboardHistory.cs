﻿using System.Collections.Generic;

namespace Pinpoint.Core.Clipboard
{
    public class ClipboardHistory : LinkedList<IClipboardEntry>
    {
        private readonly int _capacity;

        public ClipboardHistory(int capacity = 300)
        {
            _capacity = capacity;
        }

        public new void AddFirst(IClipboardEntry entry)
        {
            if (Contains(entry))
            {
                Remove(entry);
            }

            if (Count >= _capacity)
            {
                RemoveLast();
            }

            base.AddFirst(entry);
        }
    }
}
