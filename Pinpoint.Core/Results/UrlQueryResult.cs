using FontAwesome5;

namespace Pinpoint.Core.Results
{
    public class UrlQueryResult : AbstractFontAwesomeQueryResult
    {
        public UrlQueryResult(string url) : this("", url)
        {
        }

        public UrlQueryResult(string title, string url)
        {
            Title = title;
            Url = url;
            Subtitle = url;
        }

        public string Url { get; set; }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_ExternalLinkAlt;

        public override void OnSelect() => ProcessHelper.OpenUrl(Url);
    }
}
