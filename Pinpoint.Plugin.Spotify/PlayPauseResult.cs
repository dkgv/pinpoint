using FontAwesome5;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class PlayPauseResult: AbstractFontAwesomeQueryResult
    {
        public PlayPauseResult(): base("Play/pause current track") { }
        public override void OnSelect()
        {
            SpotifyClient.GetInstance().PlayPauseCurrentTrack();
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;
    }
}