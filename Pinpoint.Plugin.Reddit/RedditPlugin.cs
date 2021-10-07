using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Reddit.Models;

namespace Pinpoint.Plugin.Reddit
{
    public class RedditPlugin: IPlugin
    {
        private readonly Dictionary<string, CacheValue> _resultCache = new();
        private static readonly Regex SubRedditRegex = new(@"^r\/[0-9a-zA-z]+$");
        private const int CacheExpirationTimeInMinutes = 5;
        private const string Description = "Browse your favorite subreddits.\n\nExamples: \"r/news\", \"r/news top\", \"r/news top week\"";

        private readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://reddit.com", UriKind.Absolute)
        };

        public PluginMeta Meta { get; set; } = new("Reddit Browser", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new();

        public void Unload()
        {
            _httpClient?.Dispose();
        }

        public async Task<bool> Activate(Query query)
        {
            var queryParts = query.RawQuery.Split(' ');
            return queryParts.Length != 0 && SubRedditRegex.IsMatch(queryParts[0]);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var subReddit = query.Parts[0];

            var validParts = 1;
            string sorting = null;
            string interval = null;
            string limit = null;

            foreach (var part in query.Parts.Skip(1))
            {
                if (!int.TryParse(part, out _) && Enum.TryParse(part, true, out RedditQuerySorting _))
                {
                    sorting = part;
                    validParts++;
                }

                if (!int.TryParse(part, out _) && Enum.TryParse(part, true, out RedditQueryInterval _))
                {
                    interval = part;
                    validParts++;
                }

                if (int.TryParse(part, out var val) && val <= 100)
                {
                    limit = part;
                    validParts++;
                }
            }

            yield return new SubRedditResult(subReddit);

            var redditQuery = RedditQuery.FromStringValues(subReddit, sorting, interval, limit);

            //Matches the scenario where a user is typing one of the parameters, so we don't send requests unnecessarily with invalid parameters.
            if (validParts < query.Parts.Length)
            {
                yield break;
            }

            var result = await GetSubRedditPosts(redditQuery);

            foreach (var redditPostResult in result)
            {
                yield return new RedditResult(redditPostResult);
            }
        }

        private async Task<IEnumerable<RedditPostResultData>> GetSubRedditPosts(RedditQuery query)
        {
            var url = $"/{query.SubReddit}/{query.Sorting}.json?t={query.Interval}&limit={query.Limit}";

            if (_resultCache.TryGetValue(url, out var value))
            {
                if (value.InsertedAt.AddMinutes(CacheExpirationTimeInMinutes) > DateTime.UtcNow)
                {
                    return value.Data;
                }

                _resultCache.Remove(url);
            }

            var response = await _httpClient.GetAsync(url);

            var responseJson = await response.Content.ReadAsStringAsync();

            var apiResult = JsonConvert.DeserializeObject<RedditApiResult>(responseJson);

            var result = apiResult.Data.Children.Select(r => r.Data).ToList();

            if (!_resultCache.ContainsKey(url))
            {
                _resultCache.Add(url, new CacheValue
                {
                    InsertedAt = DateTime.UtcNow,
                    Data = result
                });
            }

            return result;
        }
    }
}
