namespace Pinpoint.Plugin.Spotify.Client;

public class PlaylistResult : SpotifyResultEntity
{
    public PlaylistResult(string name, string uri) : base(name, uri, "playlist") { }
}
