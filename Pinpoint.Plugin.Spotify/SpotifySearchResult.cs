using FontAwesome5;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifySearchResult : AbstractFontAwesomeQueryResult
    {
        private readonly SpotifyClient _spotifyClient;
        private readonly string _type;

        public SpotifySearchResult(SpotifyClient spotifyClient, string result, string uri, string type) : base(result, $"{char.ToUpper(type[0]) + type[1..]} on Spotify")
        {
            _spotifyClient = spotifyClient;
            Uri = uri;
            Options.Add(new QueueOption(_spotifyClient, "Queue", uri));
            _type = type;
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;

        public string Uri { get; }

        public override async void OnSelect()
        {
            if (!await _spotifyClient.IsCurrentlyPlaying())
            {
                await _spotifyClient.PlayPauseCurrentTrack();
            }
            _ = _spotifyClient.PlayItem(Uri);
        }
    }

    public class PausePlayResult : AbstractFontAwesomeQueryResult
    {
        private readonly SpotifyClient _spotifyClient;

        public PausePlayResult(SpotifyClient spotifyClient) : base("Play/pause current track")
        {
            _spotifyClient = spotifyClient;
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;

        public override void OnSelect()
        {
            _ = _spotifyClient.PlayPauseCurrentTrack();
        }
    }

    public class QueueOption : AbstractFontAwesomeQueryResult
    {
        private readonly SpotifyClient _spotifyClient;

        public QueueOption(SpotifyClient spotifyClient, string result, string uri) : base(result)
        {
            _spotifyClient = spotifyClient;
            Uri = uri;
        }

        public string Uri { get; }

        public override void OnSelect()
        {
            _ = _spotifyClient.QueueItem(Uri);
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_List;
    }
}