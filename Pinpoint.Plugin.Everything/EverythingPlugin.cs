using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Plugin.Everything.API;

namespace Pinpoint.Plugin.Everything
{
    public class EverythingPlugin : IPlugin
    {
        public PluginMeta Meta { get; set; } = new PluginMeta("Everything (File Search)", PluginPriority.Lowest);

        private IEverythingClient _everything;

        public void Load()
        {
            _everything = new EverythingClient(new DefaultSearchConfig());
        }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return query.RawQuery.Length >= 3;
        }

        public async IAsyncEnumerable<IQueryResult> Process(Query query)
        {
            await foreach (var result in _everything.SearchAsync(query.RawQuery, new CancellationToken()))
            {
                if (result == null)
                {
                    continue;
                }
                yield return new EverythingResult(result);
            }
        }
    }
}
