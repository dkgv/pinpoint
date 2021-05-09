using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pinpoint.Core.Results;

namespace Pinpoint.Core
{
    public interface IPlugin : IComparable<IPlugin>
    {
        public PluginMeta Meta { get; set; }

        public PluginSettings UserSettings { get; set; }

        bool IsLoaded => true;

        public Task<bool> TryLoad() => Task.FromResult(true);

        public void Unload()
        {
        }

        public Task<bool> Activate(Query query);

        public IAsyncEnumerable<AbstractQueryResult> Process(Query query);

        int IComparable<IPlugin>.CompareTo(IPlugin other)
        {
            return other.Meta.Priority.CompareTo(Meta.Priority);
        }
    }
}
