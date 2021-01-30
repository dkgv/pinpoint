using FontAwesome5;

namespace Pinpoint.Core.Results
{
    public class UrlQueryResult : AbstractFontAwesomeQueryResult
    {
        private readonly string _url;

        public UrlQueryResult(string url)
        {
            _url = url;
            Subtitle = url;
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_ExternalLinkAlt;

        public override void OnSelect()
        {
            ProcessHelper.OpenUrl(_url);
        }
    }
}
