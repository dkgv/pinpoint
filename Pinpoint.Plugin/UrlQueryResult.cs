using System.Diagnostics;
using System.Runtime.InteropServices;
using FontAwesome5;

namespace Pinpoint.Plugin
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
            try
            {
                Process.Start(_url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var modUrl = _url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {modUrl}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", _url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", _url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
