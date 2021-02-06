﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppSearchPlugin : IPlugin
    {
        private UkkonenTrie<string> _trie = new UkkonenTrie<string>();

        public PluginMeta Meta { get; set; } = new PluginMeta("App Search", PluginPriority.NextHighest);

        public void Load()
        {
            foreach (var file in LoadFileList())
            {
                var name = Path.GetFileName(file.ToLower());

                _trie.Add(name, file);

                // Support search for "Mozilla Firefox" through both "Mozilla" and "Firefox"
                if (file.Contains(" "))
                {
                    var variations = name.Split(" ");
                    foreach (var variation in variations)
                    {
                        _trie.Add(variation, file);
                    }
                }
            }
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
