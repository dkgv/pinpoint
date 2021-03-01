using System.Collections.Generic;

namespace Pinpoint.Plugin.Reddit.Models
{
    public class RedditApiResultData
    {
        public List<RedditPostResult> Children { get; set; } = new List<RedditPostResult>();
    }
}