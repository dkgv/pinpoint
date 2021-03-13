using FontAwesome5;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class ChangeTrackResult: AbstractFontAwesomeQueryResult
    {
        private readonly string _keyword;

        public ChangeTrackResult(string keyword): base(keyword == "skip" || keyword == "next" ? "Next track" : "Previous track")
        {
            _keyword = keyword;
        }
        public override void OnSelect()
        {
            if (_keyword == "skip" || _keyword == "next")
            {
                SpotifyClient.GetInstance().NextTrack();
            }
            else
            {
                SpotifyClient.GetInstance().PreviousTrack();
            }
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;
    }
}