using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Microsoft.Win32;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Bookmarks
{
    public class BookmarksPlugin : IPlugin
    {
        private BrowserType _defaultBrowserType;
        private IBookmarkExtractor _bookmarkExtractor;
        private readonly UkkonenTrie<AbstractBookmarkModel> _trie = new UkkonenTrie<AbstractBookmarkModel>();

        public PluginMeta Meta { get; set; } = new PluginMeta("Bookmarks Plugin", PluginPriority.NextHighest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool TryLoad()
        {
            _defaultBrowserType = DetectDefaultBrowser();

            if (_defaultBrowserType == BrowserType.Unknown)
            {
                return false;
            }

            _bookmarkExtractor = MapBrowserTypeToExtractor(_defaultBrowserType);
            
            foreach (var bookmarkModel in _bookmarkExtractor.Extract())
            {
                _trie.Add(bookmarkModel.Name.ToLower(), bookmarkModel);
            }

            return true;
        }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return _defaultBrowserType != BrowserType.Unknown;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            foreach (var bookmarkModel in _trie.Retrieve(query.RawQuery.ToLower()))
            {
                yield return MapBrowserToResult(_defaultBrowserType, bookmarkModel);
            }
        }

        private string GetSystemDefaultBrowserPath()
        {
            string name;
            RegistryKey regKey = null;

            try
            {
                regKey = Registry.ClassesRoot.OpenSubKey("HTTP\\shell\\open\\command", false);

                // Remove enclosing quotes
                name = regKey?.GetValue(null).ToString()?.ToLower().Replace("" + (char)34, "");

                // Check if value ends with .exe (we remove any command line arguments)
                if (name != null && !name.EndsWith("exe"))
                {
                    name = name.Substring(0, name.LastIndexOf(".exe", StringComparison.Ordinal) + 4);
                }

            }
            catch (Exception ex)
            {
                name =
                    $"ERROR: An exception of type: {ex.GetType()} occurred in method: {ex.TargetSite} in the following module: {this.GetType()}";
            }
            finally
            {
                regKey?.Close();
            }
            return name;

        }

        private BrowserType DetectDefaultBrowser()
        {
            var browser = Path.GetFileName(GetSystemDefaultBrowserPath());
            return browser switch
            {
                "firefox.exe" => BrowserType.Firefox,
                "chrome.exe" => BrowserType.Chrome,
                // "AppXq0fevzme2pys62n3e0fbqa7peapykr8v" => BrowserType.Edge,
                _ => BrowserType.Unknown
            };
        }

        private IBookmarkExtractor MapBrowserTypeToExtractor(BrowserType browser)
        {
            return browser switch
            {
                BrowserType.Chrome => new ChromeBookmarkExtractor(new ChromeBookmarkExtractor.WindowsJSONFileLocator()),
                BrowserType.Firefox => new FirefoxBookmarkExtractor(new FirefoxBookmarkExtractor.WindowsDatabaseLocator()),
                _ => null
            };
        }

        private UrlQueryResult MapBrowserToResult(BrowserType browser, AbstractBookmarkModel model)
        {
            return browser switch
            {
                BrowserType.Firefox => new FirefoxBookmarkResult(model.Name, model.Url),
                BrowserType.Chrome => new ChromeBookmarkResult(model.Name, model.Url),
                _ => new UrlQueryResult(model.Name, model.Url)
            };
        }
    }
}
