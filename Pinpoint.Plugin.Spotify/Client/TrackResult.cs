using System.Collections.Generic;
using System.Linq;

namespace Pinpoint.Plugin.Spotify.Client
{
    public class TrackResult: SpotifyResultEntity
    {
        public List<SpotifyResultEntity> Artists { get; set; } = new();

        public override string DisplayString
        {
            get
            {
                var artistsString = string.Join(", ", Artists.Select(a => a.Name));
                return $"{Name} - {artistsString}";
            }
        }

        public TrackResult(string name, string uri) : base(name, uri) { }
    }
}