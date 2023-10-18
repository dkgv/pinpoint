using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pinpoint.Win.ViewModels
{
    public class ObservableUniqueCollection<T> : ObservableCollection<T>
    {
        private readonly Dictionary<T, int> _items = new();

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                InsertItem(Count, item);
            }
        }

        public bool TryAdd(T item)
        {
            var previous = Count;
            InsertItem(Count, item);
            return Count > previous;
        }

        protected override void InsertItem(int index, T item)
        {
            if (_items.ContainsKey(item))
            {
                return;
            }

            _items[item] = index;
            base.InsertItem(index, item);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _items.Clear();
        }

        protected override void RemoveItem(int index)
        {
            _items.Remove(this[index]);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (_items.ContainsKey(item))
            {
                return;
            }

            _items.Remove(this[index]);
            _items[item] = index;

            base.SetItem(index, item);
        }

        public void RemoveWhere(Predicate<T> filter)
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                if (filter(this[i]))
                {
                    RemoveItem(i);
                }
            }
        }

        public new bool Contains(T item)
        {
            return _items.ContainsKey(item);
        }

        public new bool Remove(T item) {
            if (!_items.ContainsKey(item))
            {
                return false;
            }

            _items.Remove(item);
            base.Remove(item);

            return true;
        }
    }
}