using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public IEnumerable<AbstractBookmarkModel> Extract()
        {
            var jsonFile = _locator.Locate();
            if (string.IsNullOrEmpty(jsonFile) || !File.Exists(jsonFile))
            {
                yield break;
            }

            var jsonContent = File.ReadAllText(jsonFile);
            var json = JObject.Parse(jsonContent)["roots"]["bookmark_bar"]["children"].ToArray();
            if (json.Length == 0)
            {
                yield break;
            }

            foreach (var bookmark in json)
            {
                yield return JsonConvert.DeserializeObject<ChromeBookmarkModel>(bookmark.ToString());
            }
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