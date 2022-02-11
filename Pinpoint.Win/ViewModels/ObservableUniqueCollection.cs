using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pinpoint.Win.ViewModels
{
    public class ObservableUniqueCollection<T> : ObservableCollection<T>
    {
        private readonly HashSet<T> _hashSet = new();

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
            if (_hashSet.Add(item))
            {
                base.InsertItem(index, item);
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _hashSet.Clear();
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            _hashSet.Remove(item);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (_hashSet.Add(item))
            {
                var oldItem = this[index];
                _hashSet.Remove(oldItem);
                base.SetItem(index, item);
            }
        }
    }
}