using System;
using System.Collections.Generic;

namespace Pinpoint.Plugin.Reddit.Models
{
    public class CacheValue
    {
        public DateTime InsertedAt { get; set; }
        public List<RedditPostResultData> Data { get; set; }
    }
}