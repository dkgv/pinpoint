namespace Pinpoint.Plugin.Spotify.Client
{
    public class SearchResult
    {
        public SearchResultItems<TrackResult> Tracks { get; set; }
        public SearchResultItems<SpotifyResultEntity> Artists { get; set; }
        public SearchResultItems<SpotifyResultEntity> Albums { get; set; }
        public SearchResultItems<PlaylistResult> Playlists { get; set; }
        public SearchResultItems<SpotifyResultEntity> Episodes { get; set; }
        public SearchResultItems<SpotifyResultEntity> Shows { get; set; }
    }
}