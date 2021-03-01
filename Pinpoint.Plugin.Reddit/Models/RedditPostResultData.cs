using Newtonsoft.Json;

namespace Pinpoint.Plugin.Reddit.Models
{
    public class RedditPostResultData
    {
        public string Title { get; set; }
        public string PermaLink { get; set; }
        public string Author { get; set; }
        [JsonProperty("ups")]
        public int Upvotes { get; set; }

    }
}