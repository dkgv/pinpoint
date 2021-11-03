using System.Collections.Generic;

namespace Pinpoint.Core.Clipboard
{
    public class ClipboardHistory : LinkedList<IClipboardEntry>
    {
        private readonly int _capacity;

        public ClipboardHistory(int capacity = 100)
        {
            _capacity = capacity;
        }

        public new void AddFirst(IClipboardEntry entry)
        {
            if (Contains(entry))
            {
                return;
            }

            if (Count > _capacity)
            {
                RemoveLast();
            }

            base.AddFirst(entry);
        }
    }
}
