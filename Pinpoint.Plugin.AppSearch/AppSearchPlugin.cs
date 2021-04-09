using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppSearchPlugin : IPlugin
    {
        private UkkonenTrie<string> _trie = new UkkonenTrie<string>();
        private const string Description = "Search for installed apps. Type an app name or an abbreviation thereof.\n\nExamples: \"visual studio code\", \"vsc\"";

        // Hacky
        public static string LastQuery;

        public PluginMeta Meta { get; set; } = new PluginMeta("App Search", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool TryLoad()
        {
            AppSearchFrequency.Load();

            foreach (var file in LoadFileList())
            {
                var name = Path.GetFileName(file.ToLower());

                _trie.Add(name, file);

                if (!file.Contains(" "))
                {
                    continue;
                }

                // Support search for "Mozilla Firefox" through both "Mozilla" and "Firefox"
                var variations = name.Split(" ");
                foreach (var variation in variations)
                {
                    _trie.Add(variation, file);
                }

                // Support "Visual Studio Code" -> "VSC"
                if (variations.Length > 1)
                {
                    var fuzz = string.Join(',', variations.Select(part => part[0]).ToArray()).Replace(",", "");
                    _trie.Add(fuzz, file);
                }
            }
            
            return true;
        }

        private List<string> LoadFileList()
        {
            var files = new List<string>();

            var paths = new[]
            {
                @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
                $@"C:\Users\{Environment.UserName}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs"
            };

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    files.AddRange(Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories));
                }
            }

            return files;
        }

        public void Unload()
        {
            _trie = null;
            AppSearchFrequency.Reset();
        }

        public async Task<bool> Activate(Query query)
        {
            return query.RawQuery.Length >= 2;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var rawQuery = query.RawQuery.ToLower();
            LastQuery = rawQuery;

            foreach (var result in _trie.Retrieve(rawQuery)
                .OrderByDescending(entry => AppSearchFrequency.FrequencyOfFor(rawQuery, entry)))
            {
                yield return new AppResult(result);
            }
        }
    }
}
