using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pinpoint.Plugin;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifyClient: IDisposable
    {
        private static SpotifyClient _instance = null;
        private  readonly HttpClient _spotifyHttpClient = new HttpClient();
        private string _accessToken;

        public static SpotifyClient GetInstance()
        {
            return _instance ??= new SpotifyClient();
        }

        public void InitializeClientWithTokens(TokenResult tokens)
        {
            _accessToken = tokens.access_token;
            var settings = new SpotifyPluginSettings
            {
                RefreshToken = tokens.refresh_token, 
            };
            AppSettings.PutAndSave("spotify", settings);
        }

        public async Task<List<TrackResult>> Search(string query)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/search?q={query}&type=track&limit=5") { Headers = { {"Authorization", $"Bearer {_accessToken}"}, {"Accept", "application/json"}}};

            var response = await _spotifyHttpClient.SendAsync(message);

            if(!response.IsSuccessStatusCode)
            {
                await ExchangeRefreshToken();
                var retryMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/search?q={query}&type=track&limit=5") { Headers = { { "Authorization", $"Bearer {_accessToken}" }, { "Accept", "application/json" } } };
                response = await _spotifyHttpClient.SendAsync(retryMessage);
            }

            var bodyJson = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<TracksResult>(bodyJson);

            return result.Tracks.Items;
        }

        public async Task PlayTrack(string trackUri)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new PlayRequest {uris = new List<string> { trackUri }}),
                Encoding.UTF8, "application/json");
            var message = new HttpRequestMessage(HttpMethod.Put, "https://api.spotify.com/v1/me/player/play") { Headers = { { "Authorization", $"Bearer {_accessToken}" }, { "Accept", "application/json" } }, Content = content };
            
            var response = await _spotifyHttpClient.SendAsync(message);
            
            if(response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await ExchangeRefreshToken();
                var retryMessage = new HttpRequestMessage(HttpMethod.Put, "https://api.spotify.com/v1/me/player/play") { Headers = { { "Authorization", $"Bearer {_accessToken}" }, { "Accept", "application/json" } }, Content = content };
                await _spotifyHttpClient.SendAsync(retryMessage);
            }
        }

        private async Task ExchangeRefreshToken()
        {
            var settings = AppSettings.GetAs<SpotifyPluginSettings>("spotify");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", settings.RefreshToken },
                { "client_id", SpotifyConstants.ClientId }
            });
            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token") { Content = content };

            var response = await _spotifyHttpClient.SendAsync(request);
            if(!response.IsSuccessStatusCode)
            {
                settings.RefreshToken = null;
                AppSettings.PutAndSave("spotify", settings);
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var renewedTokens = JsonConvert.DeserializeObject<TokenResult>(responseContent);

            _accessToken = renewedTokens.access_token;
            settings.RefreshToken = renewedTokens.refresh_token;

            AppSettings.PutAndSave("spotify", settings);
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
        public List<ArtistResult> Artists { get; set; } = new List<ArtistResult>();
    }

    public class ArtistResult
    {
        public string Name { get; set; }
    }

    public class PlayRequest
    {
        public List<string> uris { get; set; }
    }
}