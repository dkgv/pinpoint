using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Bookmarks
{
    public class BookmarksPlugin : AbstractPlugin
    {
        private readonly UkkonenTrie<Tuple<BrowserType, AbstractBookmarkModel>> _trie = new();

        public override PluginManifest Manifest { get; } = new("Bookmarks Plugin")
        {
            Description = "Search for browser bookmarks from Brave, Chrome, Firefox.",
            Priority = PluginPriority.Low,
        };

        public override async Task<bool> Initialize()
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

            return true;
        }

        public override async Task<bool> ShouldActivate(Query query) => query.Raw.Length >= 2;

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var seen = new HashSet<string>();
            foreach (var bookmarkModel in _trie.Retrieve(query.Raw.ToLower()))
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
