using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Reddit
{
    public class RedditPlugin: IPlugin
    {
        private readonly Regex _subredditRegex = new Regex(@"^r\/[0-9a-zA-z]+$");

        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://reddit.com", UriKind.Absolute)
        };

        public PluginMeta Meta { get; set; } = new PluginMeta("Reddit plugin", PluginPriority.Highest);
        public bool TryLoad() => true;

        public void Unload()
        {
            _httpClient?.Dispose();
        }

        public async Task<bool> Activate(Query query)
        {
            var queryParts = query.RawQuery.Split(' ');

            if (queryParts.Length == 0) return false;

            return _subredditRegex.IsMatch(queryParts[0]);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var queryParts = query.RawQuery.Split(' ');

            var subReddit = queryParts[0];
            var sorting = queryParts.Length > 1 ? queryParts[1] : null;
            var interval = queryParts.Length > 2 ? queryParts[2] : null;

            var redditQuery = RedditQuery.FromStringValues(subReddit, sorting, interval);
            var result = await GetRedditPosts(redditQuery);

            foreach (var redditPostResult in result)
            {
                yield return new RedditResult(redditPostResult);
            }
        }

        private async Task<IEnumerable<RedditPostResultData>> GetRedditPosts(RedditQuery query)
        {
            var url = $"/{query.SubReddit}/.json?sort={query.Sorting}&t={query.Interval}";

            var response = await _httpClient.GetAsync(url);

            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResult = JsonConvert.DeserializeObject<RedditApiResult>(responseJson);

            return apiResult.Data.Children.Select(r => r.Data);
        }
    }

    public class RedditApiResult
    {
        public RedditApiResultData Data { get; set; } = new RedditApiResultData();
    }

    public class RedditApiResultData
    {
        public List<RedditPostResult> Children { get; set; } = new List<RedditPostResult>();
    }

    public class RedditPostResult
    {
        public RedditPostResultData Data { get; set; }
    }

    public class RedditPostResultData
    {
        public string Title { get; set; }
        public string PermaLink { get; set; }
        [JsonProperty("ups")]
        public int Upvotes { get; set; }
    }

    public class RedditQuery
    {
        private readonly RedditQuerySorting _querySorting;
        private readonly RedditQueryInterval _queryInterval;

        private RedditQuery(string subReddit, RedditQuerySorting querySorting, RedditQueryInterval queryInterval)
        {
            SubReddit = subReddit;
            _querySorting = querySorting;
            _queryInterval = queryInterval;
        }

        public string SubReddit { get; }

        public string Sorting => _querySorting.ToString().ToLower();

        public string Interval => _queryInterval.ToString().ToLower();

        public static RedditQuery FromStringValues(string subReddit, string sorting, string interval)
        {
            if (subReddit == null) throw new ArgumentException(nameof(subReddit));

            var querySorting = RedditQuerySorting.Hot;
            var queryInterval = RedditQueryInterval.Day;

            if(sorting != null) Enum.TryParse(sorting, ignoreCase: true, out querySorting);
            if(interval != null) Enum.TryParse(interval, ignoreCase: true, out queryInterval);

            return new RedditQuery(subReddit, querySorting, queryInterval);
        }
    }
}
