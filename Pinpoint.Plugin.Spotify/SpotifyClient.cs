using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pinpoint.Core;

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

        public async Task<List<SpotifyResult>> Search(string query, string type)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/search?q={query}&type={type}&limit=5") { Headers = { {"Authorization", $"Bearer {_accessToken}"}, {"Accept", "application/json"}}};

            var response = await _spotifyHttpClient.SendAsync(message);

            if(!response.IsSuccessStatusCode)
            {
                await ExchangeRefreshToken();
                var retryMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/search?q={query}&type=track&limit=5") { Headers = { { "Authorization", $"Bearer {_accessToken}" }, { "Accept", "application/json" } } };
                response = await _spotifyHttpClient.SendAsync(retryMessage);
            }

            var bodyJson = await response.Content.ReadAsStringAsync();

            var jObject = JsonConvert.DeserializeObject<JObject>(bodyJson);

            var result = DeserializeResponse(jObject, type);
            //var result = JsonConvert.DeserializeObject<TracksResult>(bodyJson);

            return result.Items;
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

        private SearchResult DeserializeResponse(JObject response, string type)
        {
            var results = response[$"{type}s"];

            //if (type == "track")
            //{
            //    return new SearchResult
            //    {
            //        Items = results.ToObject<List<TrackResult>>()
            //    };
            //}

            return new SearchResult
            {
                Items = results["items"]?.ToObject<List<SpotifyResult>>()
            };
        }

        public void Dispose()
        {
            _spotifyHttpClient.Dispose();
        }
    }

    public class SearchResult
    {
        public List<SpotifyResult> Items { get; set; } = new List<SpotifyResult>();
    }

    public class SpotifyResult
    {
        public SpotifyResult(string name, string uri)
        {
            Name = name;
            Uri = uri;
        }
        public string Name { get; }
        public string Uri { get; }
        public virtual string DisplayString => Name;
    }

    public class TrackResult: SpotifyResult
    {
        public List<SpotifyResult> Artists { get; set; } = new List<SpotifyResult>();

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

    public class PlayRequest
    {
        public List<string> uris { get; set; }
    }
}