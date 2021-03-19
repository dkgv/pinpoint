using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pinpoint.Core;

namespace PinPoint.Plugin.Spotify
{
    public class AuthenticationManager : IDisposable
    {
        private static readonly HttpClient AuthHttpClient = new HttpClient();
        private readonly string _codeVerifier;
        private readonly string _codeChallenge;

        public string AccessToken { get; set; }

        public AuthenticationManager()
        {
            _codeVerifier = GenerateNonce();
            _codeChallenge = GenerateCodeChallenge(_codeVerifier);
        }

        public void Dispose()
        {
            AuthHttpClient.Dispose();
        }

        public async Task<TokenResult> Authenticate()
        {
            //Start server on localhost
            var server = new LocalServer();
            server.Launch();

            //Ask user for Spotify permissions   
            OpenSpotifyPrompt();

            //Retrieve auth code from Spotify redirect   
            var ctx = server.RetrieveContext();
            var authCode = ctx.Request.QueryString["code"];

            server.Stop();
            return await ExchangeAuthCodeForToken(authCode);
        }

        public async Task<TokenResult> ExchangeAuthCodeForToken(string authCode)
        {
            var parameters = new Dictionary<string, string>
            {
                {"client_id", SpotifyConstants.ClientId},
                {"grant_type", "authorization_code"},
                {"code", authCode},
                {"redirect_uri", SpotifyConstants.RedirectUri},
                {"code_verifier", _codeVerifier}
            };

            var urlEncodedParameters = new FormUrlEncodedContent(parameters);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token") { Content = urlEncodedParameters };
            var response = AuthHttpClient.SendAsync(request).Result;
            var bodyJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TokenResult>(bodyJson);
        }

        public void OpenSpotifyPrompt()
        {
            var scopes = new List<string>
            {
                "app-remote-control",
                "streaming",
                "user-read-playback-state",
                "user-modify-playback-state",
                "user-read-currently-playing"
            };
            
            var parameters = AuthQueryParams.CreatePkceParams(SpotifyConstants.ClientId, SpotifyConstants.RedirectUri, _codeChallenge, scopes);
            ProcessHelper.OpenUrl(parameters.ToRequestUri());
        }

        private static string GenerateNonce()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz123456789";
            var random = new Random();
            var nonce = new char[128];
            for (int i = 0; i < nonce.Length; i++)
            {
                nonce[i] = chars[random.Next(chars.Length)];
            }

            return new string(nonce);
        }
        private static string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            var base64Hash = Convert.ToBase64String(hash);
            var code = Regex.Replace(base64Hash, "\\+", "-");
            code = Regex.Replace(code, "\\/", "_");
            code = Regex.Replace(code, "=+$", "");
            return code;
        }
    }
}