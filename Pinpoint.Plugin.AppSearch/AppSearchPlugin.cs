using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppSearchPlugin : IPlugin
    {
        private const string StartMenu = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs";
        private UkkonenTrie<string> _trie = new UkkonenTrie<string>();

        public PluginMeta Meta { get; set; } = new PluginMeta("App Search", PluginPriority.NextHighest);

        public void Load()
        {
            foreach (var file in Directory.GetFiles(StartMenu, "*.lnk", SearchOption.AllDirectories))
            {
                _trie.Add(file.ToLower(), file);

                // Support search for "Mozilla Firefox" through both "Mozilla" and "Firefox"
                if (file.Contains(" "))
                {
                    var variations = file.ToLower().Split(" ");
                    foreach (var variation in variations)
                    {
                        _trie.Add(variation, file);
                    }
                }
            }
        }

        public void Unload()
        {
            _trie = null;
        }

        public async Task<bool> Activate(Query query)
        {
            return query.RawQuery.Length >= 3;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            foreach (var result in _trie.Retrieve(query.RawQuery.ToLower()))
            {
                yield return new AppResult(result);
            }
        }
    }
}
