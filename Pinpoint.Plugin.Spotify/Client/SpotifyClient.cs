using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pinpoint.Core;
using PinPoint.Plugin.Spotify;

namespace Pinpoint.Plugin.Spotify.Client
{
    public class SpotifyClient: IDisposable
    {
        private static SpotifyClient _instance;
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

        public async Task<List<SpotifyResultEntity>> Search(string query, string type)
        {
            var message = CreateRequestMessage(HttpMethod.Get,
                $"https://api.spotify.com/v1/search?q={query}&type={type}&limit=5");

            var response = await SendWithRetry(message);

            var bodyJson = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonConvert.DeserializeObject<SearchResult>(bodyJson);
                return type switch
                {
                    "track" => result.Tracks.Items.Cast<SpotifyResultEntity>().ToList(),
                    "album" => result.Albums.Items,
                    "artist" => result.Artists.Items,
                    "playlist" => result.Playlists.Items,
                    "show" => result.Shows.Items,
                    "episode" => result.Episodes.Items,
                    _ => new List<SpotifyResultEntity>()
                };
            }
            catch (Exception)
            {
                return new List<SpotifyResultEntity>();
            }
        }

        public async Task PlayItem(string uri)
        {
            PlayRequest request = null;
            if (uri != null)
            {
                request = uri.Contains("track")
                    ? new PlayRequest {Uris = new List<string> {uri}}
                    : new PlayRequest {ContextUri = uri};
            }

            var message = CreateRequestMessage(HttpMethod.Put, "https://api.spotify.com/v1/me/player/play",
                uri != null ? request : null);

            var _ = await SendWithRetry(message);
        }

        public async Task PlayPauseCurrentTrack()
        {
            var isPlaying = await SpotifyCurrentlyPlaying();

            if (isPlaying)
            {
                var message = CreateRequestMessage(HttpMethod.Put, "https://api.spotify.com/v1/me/player/pause");

                var _ = await SendWithRetry(message);
            }
            else
            {
                await PlayItem(null);
            }
        }

        public async Task QueueItem(string uri)
        {
            var message =
                CreateRequestMessage(HttpMethod.Post, $"https://api.spotify.com/v1/me/player/queue?uri={uri}");
            var _ = await SendWithRetry(message);
        }

        public async Task NextTrack()
        {
            var message = CreateRequestMessage(HttpMethod.Post, "https://api.spotify.com/v1/me/player/next");
            var _ = await SendWithRetry(message);
        }

        public async Task PreviousTrack()
        {
            var message = CreateRequestMessage(HttpMethod.Post, "https://api.spotify.com/v1/me/player/previous");
            var _ = await SendWithRetry(message);
        }

        private async Task<bool> SpotifyCurrentlyPlaying()
        {
            var message = CreateRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player");
            var response = await SendWithRetry(message);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PlaybackInfoResult>(responseContent);

            return result.IsPlaying;
        }

        private async Task ExchangeRefreshToken()
        {
            var settings = AppSettings.GetOrDefault("spotify", new SpotifyPluginSettings());

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
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var renewedTokens = JsonConvert.DeserializeObject<TokenResult>(responseContent);

            if (renewedTokens?.access_token != null && renewedTokens?.refresh_token != null)
            {
                _accessToken = renewedTokens.access_token;
                settings.RefreshToken = renewedTokens.refresh_token;
            }

            AppSettings.PutAndSave("spotify", settings);
        }

        public void Dispose()
        {
            _spotifyHttpClient.Dispose();
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod httpMethod, string requestUri, object requestContent = null)
        {
            var content = httpMethod == HttpMethod.Get || requestContent == null ? null : new StringContent(JsonConvert.SerializeObject(requestContent),
                Encoding.UTF8, "application/json");
            return new HttpRequestMessage(httpMethod, requestUri) { Headers = { { "Authorization", $"Bearer {_accessToken}" }, { "Accept", "application/json" } }, Content = content };
        }

        private async Task<HttpResponseMessage> SendWithRetry(HttpRequestMessage message)
        {
            var response = await _spotifyHttpClient.SendAsync(message);

            if (response.IsSuccessStatusCode) return response;

            await ExchangeRefreshToken();

            var retryMessage = new HttpRequestMessage(message.Method, message.RequestUri)
            { Headers = { { "Authorization", $"Bearer {_accessToken}" }, { "Accept", "application/json" } }, Content = message.Method != HttpMethod.Get ? message.Content : null};

            return await _spotifyHttpClient.SendAsync(retryMessage);

        }
    }

    public class PlaybackInfoResult
    {
        [JsonProperty("is_playing")]
        public bool IsPlaying { get; set; }
    }
}