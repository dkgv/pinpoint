using System.Collections.Generic;
using Pinpoint.Core;

namespace Pinpoint.Plugin.AppSearch
{
    public static class AppSearchFrequency
    {
        private static Dictionary<string, Dictionary<string, int>> _database = new Dictionary<string, Dictionary<string, int>>();

        public static void Load()
        {
            _database = AppSettings.GetAsOrDefault("app_search", new Dictionary<string, Dictionary<string, int>>());
        }

        public static void Track(string query, string exactMatch)
        {
            if (!_database.ContainsKey(query))
            {
                _database[query] = new Dictionary<string, int>();
            }

            var frequency = _database[query];
            if (!frequency.ContainsKey(exactMatch))
            {
                frequency[exactMatch] = 1;
            }
            else
            {
                frequency[exactMatch]++;
            }

            AppSettings.PutAndSave("app_search", _database);
        }

        public static int FrequencyOfFor(string of, string @for)
        {
            if (!_database.ContainsKey(of))
            {
                return 0;
            }

            var frequency = _database[of];
            if (!frequency.ContainsKey(@for))
            {
                return 0;
            }

            return frequency[@for];
        }

        public static void Reset()
        {
            _database.Clear();
        }
    }
}