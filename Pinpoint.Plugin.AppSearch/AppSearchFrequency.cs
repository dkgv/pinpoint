using System.Collections.Generic;
using Pinpoint.Core;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppSearchFrequency
    {
        public Dictionary<string, Dictionary<string, int>> Database = new();
        private readonly IPlugin _plugin;

        public AppSearchFrequency(IPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Track(string query, string exactMatch)
        {
            if (!Database.ContainsKey(query))
            {
                Database[query] = new Dictionary<string, int>();
            }

            var frequency = Database[query];
            if (!frequency.ContainsKey(exactMatch))
            {
                frequency[exactMatch] = 1;
            }
            else
            {
                frequency[exactMatch]++;
            }

            _plugin.Storage.InternalSettings["database"] = Database;
            _plugin.Save();
        }

        public int FrequencyOfFor(string of, string @for)
        {
            if (!Database.ContainsKey(of))
            {
                return 0;
            }

            var frequency = Database[of];
            return !frequency.ContainsKey(@for) ? 0 : frequency[@for];
        }

        public void Reset()
        {
            Database.Clear();
        }
    }
}