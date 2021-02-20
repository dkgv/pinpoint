using System.Collections.Generic;

namespace Pinpoint.Core
{
    public class QueryHistory
    {
        private readonly int _maxHistorySize;

        public QueryHistory(int maxHistorySize)
        {
            _maxHistorySize = maxHistorySize;
        }

        private LinkedList<Query> Queries { get; } = new LinkedList<Query>();

        public LinkedListNode<Query> Current { get; set; }

        public void Add(Query query)
        {
            if (Current != null && Current.Value.RawQuery.Equals(query.RawQuery))
            {
                return;
            }

            Current = Queries.AddFirst(query);

            if (Queries.Count >= _maxHistorySize)
            {
                Queries.RemoveLast();
            }
        }
    }
}