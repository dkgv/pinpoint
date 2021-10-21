using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Pinpoint.Plugin.Everything.API
{
    public interface IEverythingClient : IDisposable
    {
        public ISearchConfig Config { get; set; }

        public IAsyncEnumerable<QueryResultItem> SearchAsync(string query, [EnumeratorCancellation] CancellationToken ct);
    }
}
