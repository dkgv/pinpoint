using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pinpoint.Core.Results;

namespace Pinpoint.Core
{
    public class EmptyPlugin : IPlugin
    {
        public PluginMeta Meta { get; set; }

        public PluginSettings UserSettings { get; set; }
        
        public bool TryLoad() => true;

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            throw new NotImplementedException();
        }
    }
}
