namespace Pinpoint.Plugin.Spotify.Client
{
    public class SpotifyResultEntity
    {
        public SpotifyResultEntity(string name, string uri, string type)
        {
            Name = name;
            Uri = uri;
            Type = type;
        }

        public string Name { get; }
        public string Uri { get; }
        public virtual string DisplayString => Name;
        public string Type { get; set; }
    }
}