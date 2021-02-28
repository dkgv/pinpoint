using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Reddit
{
    public class RedditResult: AbstractFontAwesomeQueryResult
    {
        private readonly RedditPostResultData _redditPost;

        public RedditResult(RedditPostResultData redditPost): base(redditPost.Title)
        {
            _redditPost = redditPost;
            Subtitle = $"{redditPost.Upvotes} upvotes";
        }
        public override void OnSelect()
        {
            ProcessHelper.OpenUrl($"https://reddit.com{_redditPost.PermaLink}");
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Reddit;
    }
}