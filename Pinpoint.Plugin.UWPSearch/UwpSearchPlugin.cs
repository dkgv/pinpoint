using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.AppSearch;

namespace Pinpoint.Plugin.UWPSearch
{
    public class UwpSearchPlugin : IPlugin
    {
        private UkkonenTrie<string> _trie = new();
        private const string Description = "Search for and run Universal Windows Platform apps.";
        
        public static string LastQuery;
        
        public PluginMeta Meta { get; set; } = new("UWP App Search", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new();

        public bool IsLoaded { get; set; }

        public Task<bool> TryLoad()
        {
            AppSearchFrequency.Load();

            var powerShell = PowerShell.Create();
            powerShell.AddScript("Get-StartApps | Where-Object { $_.AppID.Contains(\"!\") }");
            var result = powerShell.Invoke();
            foreach (var psObject in result)
            {
                var name = psObject.Members["Name"].Value.ToString();
                var appid = psObject.Members["AppID"].Value.ToString();
            
                if (appid == null || name == null) continue;
                
                var appValue = $"{name}#{appid}";
                _trie.Add(name.ToLower(), appValue);
                
                var variations = name.ToLower().Split(" ");
                foreach (var variation in variations)
                {
                    _trie.Add(variation, appValue);
                }
            }

            IsLoaded = true;
            return Task.FromResult(IsLoaded);
        }
        
        public void Unload()
        {
            _trie = null;
            AppSearchFrequency.Reset();
        }

        public Task<bool> Activate(Query query) => Task.FromResult(query.RawQuery.Length >= 2);

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, CancellationToken ct)
        {
            var rawQuery = query.RawQuery.ToLower();
            LastQuery = rawQuery;
            
            foreach (var result in _trie.Retrieve(rawQuery)
                         .OrderByDescending(entry => AppSearchFrequency.FrequencyOfFor(rawQuery, entry)))
            {
                yield return new UwpAppResult(result);
            }
        }
    }
}