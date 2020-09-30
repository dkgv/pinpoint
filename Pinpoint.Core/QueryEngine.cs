using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pinpoint.Core.Sources;

namespace Pinpoint.Core
{
    public static class QueryEngine
    {
        public static readonly List<ISource> Sources = new List<ISource>();

        public static bool AddSources(IEnumerable<ISource> sources)
        {
            return sources.All(AddSource);
        }

        public static bool AddSource(ISource source)
        {
            // Prevents duplicate sources
            if (Sources.Contains(source))
            {
                return false;
            }

            Sources.Add(source);
            AppSettings.PutAndSave("sources", Sources);

            return true;
        }

        public static void RemoveSource(ISource source)
        {
            Sources.Remove(source);
            AppSettings.PutAndSave("sources", Sources);
        }

        public static async IAsyncEnumerable<ISource> Process(Query query)
        {
            foreach (var source in Sources)
            {
                if (await source.Applicable(query))
                {
                    yield return source;
                }
            }
        }
    }
}
