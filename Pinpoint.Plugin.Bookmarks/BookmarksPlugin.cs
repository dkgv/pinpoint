﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Bookmarks
{
    public class BookmarksPlugin : IPlugin
    {
        private const string Description = "Search for browser bookmarks from Brave, Chrome, Firefox.";

        private readonly UkkonenTrie<Tuple<BrowserType, AbstractBookmarkModel>> _trie = new();

        public PluginMeta Meta { get; set; } = new("Bookmarks Plugin", Description, PluginPriority.NextHighest);

        public PluginSettings UserSettings { get; set; } = new();

        public bool IsLoaded { get; set; }

        public async Task<bool> TryLoad()
        {
            foreach (var browserType in Enum.GetValues(typeof(BrowserType)).Cast<BrowserType>())
            {
                var bookmarkExtractor = MapBrowserTypeToExtractor(browserType);
                foreach (var bookmarkModel in await bookmarkExtractor.Extract().ConfigureAwait(false))
                {
                    var tuple = new Tuple<BrowserType, AbstractBookmarkModel>(browserType, bookmarkModel);
                    _trie.Add(bookmarkModel.Name.ToLower(), tuple);
                }
            }

            IsLoaded = true;
            return IsLoaded;
        }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query) => query.RawQuery.Length >= 2;

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var seen = new HashSet<string>();
            foreach (var bookmarkModel in _trie.Retrieve(query.RawQuery.ToLower()))
            {
                if (seen.Add(bookmarkModel.Item2.Url))
                {
                    yield return MapBrowserToResult(bookmarkModel.Item1, bookmarkModel.Item2);
                }
            }
        }

        private IBookmarkExtractor MapBrowserTypeToExtractor(BrowserType browser)
        {
            return browser switch
            {
                BrowserType.Chrome => new ChromeBookmarkExtractor(new ChromeBookmarkExtractor.WindowsJSONFileLocator()),
                BrowserType.Firefox => new FirefoxBookmarkExtractor(new FirefoxBookmarkExtractor.WindowsDatabaseLocator()),
                BrowserType.Brave => new BraveBookmarkExtractor(new BraveBookmarkExtractor.WindowsJSONFileLocator()),
                _ => null
            };
        }

        private UrlQueryResult MapBrowserToResult(BrowserType browser, AbstractBookmarkModel model)
        {
            return browser switch
            {
                BrowserType.Firefox => new FirefoxBookmarkResult(model.Name, model.Url),
                BrowserType.Chrome => new ChromeBookmarkResult(model.Name, model.Url),
                BrowserType.Brave => new DefaultBookmarkResult(model.Name, model.Url),
                _ => new UrlQueryResult(model.Name, model.Url)
            };
        }
    }
}
