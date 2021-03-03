using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Reddit
{
    public class SubRedditResult: AbstractFontAwesomeQueryResult
    {
        private readonly string _subReddit;

        public SubRedditResult(string subReddit): base("Open subreddit")
        {
            _subReddit = subReddit;
            Subtitle = subReddit;
        }
        public override void OnSelect()
        {
            ProcessHelper.OpenUrl($"https://reddit.com/{_subReddit}");
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Reddit;
    }
}