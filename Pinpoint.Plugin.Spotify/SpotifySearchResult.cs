using FontAwesome5;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifySearchResult : AbstractFontAwesomeQueryResult
    {
        private readonly string _type;

        public SpotifySearchResult(string result, string uri, string type) : base(result, $"{char.ToUpper(type[0]) + type[1..]} on Spotify")
        {
            Uri = uri;
            Options.Add(new QueueOption("Queue", uri));
            _type = type;
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;

        public string Uri { get; }

        public override async void OnSelect()
        {
            if (!await SpotifyClient.GetInstance().IsCurrentlyPlaying())
            {
                await SpotifyClient.GetInstance().PlayPauseCurrentTrack();
            }
            _ = SpotifyClient.GetInstance().PlayItem(Uri);
        }
    }

    public class PausePlayResult : AbstractFontAwesomeQueryResult
    {
        public PausePlayResult() : base("Play/pause current track")
        {

        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;

        public override void OnSelect()
        {
            _ = SpotifyClient.GetInstance().PlayPauseCurrentTrack();
        }
    }

    public class QueueOption : AbstractFontAwesomeQueryResult
    {
        public QueueOption(string result, string uri) : base(result)
        {
            Uri = uri;
        }

        public string Uri { get; }

        public override void OnSelect()
        {
            _ = SpotifyClient.GetInstance().QueueItem(Uri);
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_List;
    }
}