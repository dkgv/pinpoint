using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.Spotify.Client
{
    public class PlayRequest
    {
        [JsonProperty("uris")]
        public List<string> Uris { get; set; }
        [JsonProperty("context_uri")]
        public string ContextUri { get; set; }
    }
}