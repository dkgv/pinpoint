using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.UrlLauncher
{
    public class UrlLauncherResult: AbstractFontAwesomeQueryResult
    {
        private readonly string _url;

        public UrlLauncherResult(string url): base($"Launch {url}")
        {
            _url = url;
        }
        public override void OnSelect()
        {
            ProcessHelper.OpenUrl(_url);
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_ExternalLinkAlt;
    }
}