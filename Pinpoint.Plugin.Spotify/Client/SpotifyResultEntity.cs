namespace Pinpoint.Plugin.Spotify.Client
{
    public class SpotifyResultEntity
    {
        public SpotifyResultEntity(string name, string uri)
        {
            Name = name;
            Uri = uri;
        }
        public string Name { get; }
        public string Uri { get; }
        public virtual string DisplayString => Name;
    }
}