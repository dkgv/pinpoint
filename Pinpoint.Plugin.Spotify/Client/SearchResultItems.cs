using System.Collections.Generic;

namespace Pinpoint.Plugin.Spotify.Client
{
    public class SearchResultItems<T> where T : SpotifyResultEntity
    {
        public List<T> Items { get; set; }
    }
}