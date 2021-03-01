using System;

namespace Pinpoint.Plugin.Reddit.Models
{
    public class RedditQuery
    {
        private readonly RedditQuerySorting _querySorting;
        private readonly RedditQueryInterval _queryInterval;

        private RedditQuery(string subReddit, RedditQuerySorting querySorting, RedditQueryInterval queryInterval, int limit)
        {
            SubReddit = subReddit;
            _querySorting = querySorting;
            _queryInterval = queryInterval;
            Limit = limit;
        }

        public string SubReddit { get; }

        public string Sorting => _querySorting.ToString().ToLower();

        public string Interval => _queryInterval.ToString().ToLower();
        public int Limit { get; }

        private static RedditQuery Create(string subReddit, RedditQuerySorting sorting, RedditQueryInterval interval,
            int limit = 10)
        {
            if (subReddit == null) throw new ArgumentException(nameof(subReddit));

            return new RedditQuery(subReddit, sorting, interval, limit);
        }

        public static RedditQuery FromStringValues(string subReddit, string sorting, string interval, string limit)
        {
            if (subReddit == null) throw new ArgumentException(nameof(subReddit));

            var querySorting = RedditQuerySorting.Hot;
            var queryInterval = RedditQueryInterval.Day;

            if (sorting != null)
            {
                var parseable = Enum.TryParse(sorting, ignoreCase: true, out querySorting);
                if (!parseable)
                {
                    querySorting = RedditQuerySorting.InvalidValue;
                }
            }

            if (interval != null)
            {
                var parseable = Enum.TryParse(interval, ignoreCase: true, out queryInterval);
                if (!parseable)
                {
                    queryInterval = RedditQueryInterval.InvalidValue;
                }
            }

            var queryLimit = 10;

            if (int.TryParse(limit, out var parsedLimit))
            {
                queryLimit = parsedLimit;
            }

            return new RedditQuery(subReddit, querySorting, queryInterval, queryLimit);
        }
    }
}