using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pinpoint.Plugin
{
    public interface IPlugin : IComparable<IPlugin>
    {
        public PluginMeta Meta { get; set; }

        public void Load();

        public void Unload();

        public Task<bool> Activate(Query query);

        public IAsyncEnumerable<AbstractQueryResult> Process(Query query);

        int IComparable<IPlugin>.CompareTo(IPlugin other)
        {
            return other.Meta.Priority.CompareTo(Meta.Priority);
        }
    }
}
