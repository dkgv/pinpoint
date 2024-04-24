using System.Collections.Generic;
using Newtonsoft.Json;
using Pinpoint.Core;

namespace Pinpoint.Plugin.AppSearch;

public class AppSearchFrequency
{
    public Dictionary<string, Dictionary<string, int>> Database = new();
    private readonly AbstractPlugin _plugin;

    public AppSearchFrequency(AbstractPlugin plugin)
    {
        _plugin = plugin;

        if (_plugin.Storage.Internal.ContainsKey("database"))
        {
            Load();
        }
    }

    private void Load()
    {
        Database = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(
            _plugin.Storage.Internal["database"].ToString());
    }

    public void Hit(string query, string exactMatch)
    {
        void Increment(string subQuery, string exactMatch)
        {
            if (!Database.ContainsKey(subQuery))
            {
                Database[subQuery] = new Dictionary<string, int>();
            }

            var frequency = Database[subQuery];
            if (!frequency.ContainsKey(exactMatch))
            {
                frequency[exactMatch] = 1;
            }
            else
            {
                frequency[exactMatch]++;
            }
        }

        for (var i = 1; i <= query.Length; i++)
        {
            var subQuery = query[..i];
            Increment(subQuery, exactMatch);
        }
        
        _plugin.Storage.Internal["database"] = Database;
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