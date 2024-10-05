using FontAwesome5;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class PlayPauseResult: AbstractFontAwesomeQueryResult
    {
        private readonly SpotifyClient _spotifyClient;

        public PlayPauseResult(SpotifyClient spotifyClient): base("Play/pause current track") 
        { 
            _spotifyClient = spotifyClient;
        }

        public override void OnSelect()
        {
            _ = _spotifyClient.PlayPauseCurrentTrack();
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;
    }
}