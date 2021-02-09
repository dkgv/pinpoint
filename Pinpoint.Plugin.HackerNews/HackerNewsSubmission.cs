using Newtonsoft.Json;

namespace Pinpoint.Plugin.HackerNews
{
    public class HackerNewsSubmission
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("by")]
        public string Author { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("descendants")]
        public int Descendants { get; set; }
    }
}
