using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppSearchPlugin : IPlugin
    {
        private UkkonenTrie<string> _trie = new();
        private const string Description = "Search for installed apps. Type an app name or an abbreviation thereof.\n\nExamples: \"visual studio code\", \"vsc\"";

        public PluginMeta Meta { get; set; } = new("App Search", Description, PluginPriority.NextHighest);

        public PluginSettings UserSettings { get; set; } = new();

        public bool IsLoaded { get; set; }

        public Task<bool> TryLoad()
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
                    var appAcronymLetters = variations.Where(part => part.Length > 0)
                        .Select(part => part[0])
                        .ToArray();
                    var acronym = string.Join(',', appAcronymLetters).Replace(",", "");
                    _trie.Add(acronym, file);
                }
            }

            IsLoaded = true;
            return Task.FromResult(IsLoaded);
        }

        private List<string> LoadFileList()
        {
            var files = new List<string>();

            var paths = new[]
            {
                @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
                $@"C:\Users\{Environment.UserName}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs"
            };

            foreach (var path in paths.Where(Directory.Exists))
            {
                var shortcuts = Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories);
                files.AddRange(shortcuts);
            }

            return files;
        }

        public void Unload()
        {
            _trie = null;
            AppSearchFrequency.Reset();
        }

        public async Task<bool> Activate(Query query) => query.RawQuery.Length >= 2;

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var rawQuery = query.RawQuery.ToLower();

            foreach (var result in _trie.Retrieve(rawQuery)
                .OrderByDescending(entry => AppSearchFrequency.FrequencyOfFor(rawQuery, entry)))
            {
                yield return new AppResult(result, rawQuery);
            }
        }
    }
}
