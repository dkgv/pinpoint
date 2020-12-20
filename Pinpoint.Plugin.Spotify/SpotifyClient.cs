using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifyClient: IDisposable
    {
        private static SpotifyClient _instance = null;
        private string _refreshToken;
        private  readonly HttpClient _spotifyHttpClient = new HttpClient();
        private string _accessToken;

        public static SpotifyClient GetInstance()
        {
            return _instance ??= new SpotifyClient();
        }

        public void InitializeClientWithTokens(ExchangeAuthorizationCodeResult tokens)
        {
            _accessToken = tokens.access_token;
            _refreshToken = tokens.refresh_token;
        }

        public async Task<List<TrackResult>> Search(string query)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/search?q={query}&type=track&limit=5") { Headers = { {"Authorization", $"Bearer {_accessToken}"}, {"Accept", "application/json"}}};

            var res = await _spotifyHttpClient.SendAsync(message);
            var bodyJson = await res.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<TracksResult>(bodyJson);

            return result.Tracks.Items;
        }

        public async Task PlayTrack(string trackUri)
        {


            var content = new StringContent(JsonConvert.SerializeObject(new PlayRequest {uris = new List<string> { trackUri }}),
                Encoding.UTF8, "application/json");
            var message = new HttpRequestMessage(HttpMethod.Put, "https://api.spotify.com/v1/me/player/play") { Headers = { { "Authorization", $"Bearer {_accessToken}" }, { "Accept", "application/json" } }, Content = content };

            await _spotifyHttpClient.SendAsync(message);
        }


        public void Dispose()
        {
            _spotifyHttpClient.Dispose();
        }
    }

    public class TracksResult
    {
        public Track Tracks { get; set; }
    }

    public class Track
    {
        public List<TrackResult> Items { get; set; }
    }


    public class TrackResult
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }

    public class PlayRequest
    {
        public List<string> uris { get; set; }
    }
}