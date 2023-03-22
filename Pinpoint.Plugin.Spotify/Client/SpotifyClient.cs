using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pinpoint.Core;
using PinPoint.Plugin.Spotify;

namespace Pinpoint.Plugin.Spotify.Client
{
    public class SpotifyClient: IDisposable
    {
        private static SpotifyClient _instance;
        public static IPlugin Plugin;
        private static readonly HttpClient SpotifyHttpClient = new();
        private string _accessToken;
        private const string SpotifyApiBaseUrl = "https://api.spotify.com/v1";

        public static SpotifyClient GetInstance()
        {
            return _instance ??= new SpotifyClient();
        }

        public async Task InitializeClientWithTokens(TokenResult tokens)
        {
            _accessToken = tokens.access_token;

            Plugin.Storage.InternalSettings["refresh_token"] = tokens.refresh_token;
            await Plugin.Save();
        }

        public async Task<List<SpotifyResultEntity>> Search(string query, string type)
        {
            var message = CreateRequestMessage(HttpMethod.Get,
                $"{SpotifyApiBaseUrl}/search?q={query}&type={type}&limit=5");

            var response = await SendWithRetry(message);

            var bodyJson = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonConvert.DeserializeObject<SearchResult>(bodyJson);
                return type switch
                {
                    "track" => result.Tracks?.Items.Cast<SpotifyResultEntity>().ToList(),
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

            var message = CreateRequestMessage(HttpMethod.Put, $"{SpotifyApiBaseUrl}/me/player/play",
                uri != null ? request : null);

            var response = await SendWithRetry(message);

            if (!response.IsSuccessStatusCode)
            {
                var availableDevices = await GetAvailableDevices();

                var deviceToPlayOn = availableDevices.FirstOrDefault();
                if (deviceToPlayOn == null) return;

                var newMessage = CreateRequestMessage(HttpMethod.Put,
                    $"{SpotifyApiBaseUrl}/me/player/play?device_id={deviceToPlayOn.Id}");
                await SpotifyHttpClient.SendAsync(newMessage);
            }
        }

        public async Task PlayPauseCurrentTrack()
        {
            var isPlaying = await IsCurrentlyPlaying();

            if (isPlaying)
            {
                var message = CreateRequestMessage(HttpMethod.Put, $"{SpotifyApiBaseUrl}/me/player/pause");

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
                CreateRequestMessage(HttpMethod.Post, $"{SpotifyApiBaseUrl}/me/player/queue?uri={uri}");
            var _ = await SendWithRetry(message);
        }

        public async Task NextTrack()
        {
            var message = CreateRequestMessage(HttpMethod.Post, $"{SpotifyApiBaseUrl}/me/player/next");
            var _ = await SendWithRetry(message);
        }

        public async Task PreviousTrack()
        {
            var message = CreateRequestMessage(HttpMethod.Post, $"{SpotifyApiBaseUrl}/me/player/previous");
            var _ = await SendWithRetry(message);
        }

        private async Task<List<PlayerDevice>> GetAvailableDevices()
        {
            var message = CreateRequestMessage(HttpMethod.Get, $"{SpotifyApiBaseUrl}/me/player/devices");
            var response = await SendWithRetry(message);

            if (!response.IsSuccessStatusCode) return new List<PlayerDevice>();

            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<AvailableDevicesResult>(responseJson);

            return result.Devices;
        }

        public async Task<bool> IsCurrentlyPlaying()
        {
            var message = CreateRequestMessage(HttpMethod.Get, $"{SpotifyApiBaseUrl}/me/player");
            var response = await SendWithRetry(message);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PlaybackInfoResult>(responseContent);

            return result is {IsPlaying: true};
        }

        private async Task ExchangeRefreshToken()
        {
            var refreshToken = Plugin.Storage.InternalSettings["refresh_token"].ToString();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", SpotifyConstants.ClientId }
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token") { Content = content };
            var response = await SpotifyHttpClient.SendAsync(request);
            if(!response.IsSuccessStatusCode)
            {
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var renewedTokens = JsonConvert.DeserializeObject<TokenResult>(responseContent);

            if (renewedTokens?.access_token != null && renewedTokens?.refresh_token != null)
            {
                _accessToken = renewedTokens.access_token;
                refreshToken = renewedTokens.refresh_token;
            }

            Plugin.Storage.InternalSettings["refresh_token"] = refreshToken;
            await Plugin.Save();
        }

        public void Dispose()
        {
            SpotifyHttpClient.Dispose();
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod httpMethod, string requestUri, object requestContent = null)
        {
            var content = httpMethod == HttpMethod.Get || requestContent == null ? null : new StringContent(JsonConvert.SerializeObject(requestContent),
                Encoding.UTF8, "application/json");
            return new HttpRequestMessage(httpMethod, requestUri) { Headers = { { "Authorization", $"Bearer {_accessToken}" }, { "Accept", "application/json" } }, Content = content };
        }

        private async Task<HttpResponseMessage> SendWithRetry(HttpRequestMessage message)
        {
            var response = await SpotifyHttpClient.SendAsync(message);

            if (response.IsSuccessStatusCode) return response;

            await ExchangeRefreshToken();

            var retryMessage = new HttpRequestMessage(message.Method, message.RequestUri)
            {
                Headers = {
                    { "Authorization", $"Bearer {_accessToken}" },
                    { "Accept", "application/json" }
            }, Content = message.Method != HttpMethod.Get ? message.Content : null};

            return await SpotifyHttpClient.SendAsync(retryMessage);

        }
    }

    public class PlaybackInfoResult
    {
        [JsonProperty("is_playing")]
        public bool IsPlaying { get; set; }
    }

    public class AvailableDevicesResult
    {
        [JsonProperty("devices")]
        public List<PlayerDevice> Devices { get; set; }
    }

    public class PlayerDevice
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}