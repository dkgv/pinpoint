using System;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifySearchResult : AbstractFontAwesomeQueryResult
    {
        public SpotifySearchResult(string result, string uri) : base(result)
        {
            Uri = uri;
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Music;

        public string Uri { get; }
        public override void OnSelect()
        {
            SpotifyClient.GetInstance().PlayTrack(Uri);
        }
    }

    public class PausePlayResult : AbstractFontAwesomeQueryResult
    {
        public PausePlayResult(): base("Play/pause current track")
        {
            
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Play;

        public override void OnSelect()
        {
            SpotifyClient.GetInstance().PlayPauseTrack();
        }
    }
}