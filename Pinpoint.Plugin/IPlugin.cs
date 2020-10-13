using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pinpoint.Plugin
{
    public interface IPlugin
    {
        public PluginMeta Meta { get; set; }

        public void Load();

        public void Unload();

        public Task<bool> Activate(Query query);

        public IAsyncEnumerable<IQueryResult> Process(Query query);
    }
}
