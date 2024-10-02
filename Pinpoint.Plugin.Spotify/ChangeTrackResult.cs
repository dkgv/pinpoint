using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class ChangeTrackResult: AbstractFontAwesomeQueryResult
    {
        private readonly SpotifyClient _spotifyClient;
        private readonly string _keyword;

        public ChangeTrackResult(SpotifyClient spotifyClient, string keyword): base(keyword == "skip" || keyword == "next" ? "Next track" : "Previous track")
        {
            _spotifyClient = spotifyClient;
            _keyword = keyword;
        }

        public override void OnSelect()
        {
            if (_keyword == "skip" || _keyword == "next")
            {
                Task.Run(() =>_spotifyClient.NextTrack());
            }
            else
            {
                Task.Run(() => _spotifyClient.PreviousTrack());
            }
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;
    }
}