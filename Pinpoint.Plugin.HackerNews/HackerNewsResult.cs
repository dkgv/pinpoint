using System;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.HackerNews
{
    public class HackerNewsResult : UrlQueryResult
    {
        private const string BaseUrl = "https://news.ycombinator.com/item?id={0}";

        public HackerNewsResult(int n, HackerNewsSubmission submission) : base(string.Format(BaseUrl, submission.Id))
        {
            Title = n + ". " + submission.Title;
            Subtitle = submission.Score + " points by " + submission.Author + " " + GetHoursAgo(submission) + " hours ago | " + submission.Descendants + " comments";
                
            // Will be null if no external link was submitted
            if (!string.IsNullOrEmpty(submission.Url))
            {
                Title += " (" + GetBaseUrl(submission) + ")";
                Options.Add(new UrlQueryResult("Open " + submission.Url, submission.Url));
            }
        }

        private long GetHoursAgo(HackerNewsSubmission submission)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var then = submission.Time;
            var sSince = now - then;
            var mSince = sSince / 60;
            var hSince = mSince / 60;
            return hSince;
        }

        private string GetBaseUrl(HackerNewsSubmission submission)
        {
            var uri = new Uri(submission.Url);
            return uri.Host;
        }

        public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Brands_HackerNewsSquare;
    }
}