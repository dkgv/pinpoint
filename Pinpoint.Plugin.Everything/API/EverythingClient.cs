using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Everything.API
{
    public class EverythingClient : IEverythingClient
    {
        private readonly SearchProvider _searchProvider = new SearchProvider();
        private readonly ResultProvider _resultProvider = new ResultProvider();

        public EverythingClient(ISearchConfig config)
        {
            Config = config;
        }

        public void Dispose()
        {
            EverythingDll.Everything_Reset();
        }

        public ISearchConfig Config { get; set; }

        public async IAsyncEnumerable<QueryResultItem> SearchAsync(string query, [EnumeratorCancellation] CancellationToken ct)
        {
            _ = Task.Run(() =>
            {
                _searchProvider.Search(query, Config);
            }, ct);

            await foreach (var item in _resultProvider.GetCurrentResult(ct))
            {
                yield return item;
            }
        }
    }
}
