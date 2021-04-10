using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pinpoint.Plugin.Bookmarks
{
    public class ChromeBookmarkExtractor : IBookmarkExtractor
    {
        private readonly IBookmarkFileLocator _locator;

        public ChromeBookmarkExtractor(IBookmarkFileLocator locator)
        {
            _locator = locator;
        }

        public async Task<IEnumerable<AbstractBookmarkModel>> Extract()
        {
            var jsonFile = _locator.Locate();
            if (string.IsNullOrEmpty(jsonFile) || !File.Exists(jsonFile))
            {
                return new List<AbstractBookmarkModel>();
            }

            var jsonContent = await File.ReadAllTextAsync(jsonFile).ConfigureAwait(false);
            var json = JObject.Parse(jsonContent)["roots"]["bookmark_bar"]["children"].ToArray();
            if (json.Length == 0)
            {
                return new List<AbstractBookmarkModel>();
            }

            return json.Select(bookmark => JsonConvert.DeserializeObject<ChromeBookmarkModel>(bookmark.ToString()));
        }

        private class ChromeBookmarkModel : AbstractBookmarkModel
        {
            [JsonProperty("name")]
            public override string Name { get; set; }

            [JsonProperty("url")]
            public override string Url { get; set; }
        }

        public class WindowsJSONFileLocator : IBookmarkFileLocator
        {
            public string Locate()
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Google",
                    "Chrome",
                    "User Data",
                    "Default",
                    "Bookmarks"
                );

                return path;
            }
        }
    }
}