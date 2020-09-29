using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pinpoint.Core
{
    public interface ISource
    {
        public bool Applicable(Query query);

        public Task<List<IQueryResult>> Pinpoint(Query query);
    }
}
