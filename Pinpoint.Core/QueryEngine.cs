using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pinpoint.Core
{
    public static class QueryEngine
    {
        private static readonly List<ISource> Sources = new List<ISource>();

        public static void AddSource(ISource source)
        {
            Sources.Add(source);
        }

        public static void RemoveSource(ISource source)
        {
            Sources.Remove(source);
        }

        public static async Task<List<IQueryResult>> Lookup(Query query)
        {
            var tasks = new List<Task<List<IQueryResult>>>();
            foreach (var source in Sources.Where(src => src.Applicable(query)))
            {
                tasks.Add(source.Pinpoint(query));
            }

            var results = await Task.WhenAll(tasks.ToArray());
            return results.Aggregate(new List<IQueryResult>(), (a, b) => a.Concat(b).ToList());
        }
    }
}
