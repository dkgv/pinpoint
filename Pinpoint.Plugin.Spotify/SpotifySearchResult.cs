﻿using FontAwesome5;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifySearchResult : AbstractFontAwesomeQueryResult
    {
        public SpotifySearchResult(string result, string uri) : base(result)
        {
            Uri = uri;

            Options.Add(new QueueOption("Queue", uri));
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;

        public string Uri { get; }
        public override void OnSelect()
        {
            SpotifyClient.GetInstance().PlayItem(Uri);
        }
    }

    public class PausePlayResult : AbstractFontAwesomeQueryResult
    {
        public PausePlayResult(): base("Play/pause current track")
        {
            
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;

        public override void OnSelect()
        {
            SpotifyClient.GetInstance().PlayPauseCurrentTrack();
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
            SpotifyClient.GetInstance().QueueItem(Uri);
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_List;
    }
}