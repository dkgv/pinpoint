using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Plugin;
using System.Text.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifyPlugin : IPlugin
    {
        private AuthenticationManager _authManager = new AuthenticationManager();
        private SpotifyClient _client;
        
        public PluginMeta Meta { get; set; } = new PluginMeta("Spotify Controller");

        public void Load()
        {
            //Check auth here to ensure we always have a valid refresh token
            //Case 1: no refresh token, no access token - prompt login in browser
            //Case 2: refresh token, no access token - attempt to exchange refresh token, if that fails, prompt login in browser
            //Case 3: refresh token, access token - use access token, if response status code is 401, exchange refresh token and try again
            
            _client = new SpotifyClient(_authManager.Authenticate());
        }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            //Need some way to distinguish between play/queue/next actions
            return false;
        }

        public IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            //Search results go here I guess
            throw new NotImplementedException();
        }
    }

    public class SpotifyClient 
    {
        public SpotifyClient(ExchangeAuthorizationCodeResult result)
        {
        }
    }

    public class LocalServer
    {
        private HttpListener _listener = new HttpListener { 
            Prefixes = { "http://localhost:5000/" }
        };

        public void Launch()
        {
            _listener.Start();
        }
        
        public HttpListenerContext RetrieveContext()
        {
            var ctx = _listener.GetContext();
            _listener.Stop();
            return ctx;
        }
    }

    public class AuthenticationManager : IDisposable
    {
        private const string ClientId = "4f8b3722704c4cfba27db654aaebd4cc";
        private static readonly HttpClient HttpClient = new HttpClient();
        
        public void Dispose()
        {
            HttpClient.Dispose();
        }

        public ExchangeAuthorizationCodeResult Authenticate()
        {
            // Start server on localhost
            var server = new LocalServer();
            server.Launch();

            // Compute code challenge
            var codeVerifier = GenerateRandomURLSafeString();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);

            // Ask user for Spotify permissions            
            OpenSpotifyPrompt(codeChallenge);

            // Retrieve auth code from Spotify redirect
            var ctx = server.RetrieveContext();
            var authCode = ctx.Request.QueryString["code"];

            return ExchangeAuthCodeForToken(authCode, codeVerifier);
        }

        public ExchangeAuthorizationCodeResult ExchangeAuthCodeForToken(string authCode, string codeVerifier)
        {
            /*var paramz = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", ClientId ),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authCode),
                new KeyValuePair<string, string>("redirect_uri", "http://localhost:5000"),
                new KeyValuePair<string, string>("code_verifier", codeVerifier)
            };

            var qs = string.Join("&", paramz.Select(x => $"{x.Key}={x.Value}").ToArray());
            var content = new FormUrlEncodedContent(paramz);
            //HttpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
            var response = HttpClient.PostAsync($"https://accounts.spotify.com/api/token?{qs}", content).Result;
            var bodyJson = response.Content.ReadAsStringAsync().Result;

            var lort = new OAuthClient().RequestToken(new PKCETokenRequest(ClientId, authCode, new Uri("http://localhost:5000"), codeVerifier)).Result;
            Debug.WriteLine(bodyJson);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Something went wrong while trying to authenticate with Spotify.");
            }

            return JsonSerializer.Deserialize<ExchangeAuthorizationCodeResult>(bodyJson);
        }

        public void OpenSpotifyPrompt(string codeChallenge)
        {
            var scopes = new List<string>
            {
                "app-remote-control",
                "streaming",
                "user-read-playback-state",
                "user-modify-playback-state",
                "user-read-currently-playing"
            };
            
            var paramz = AuthQueryParams.CreatePKCEParams(ClientId, "http://localhost:5000", codeChallenge, scopes);
            ProcessHelper.OpenUrl(paramz.ToRequestUri());
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Base64Util.UrlEncode(hash);
        }

        private static string GenerateRandomURLSafeString(int length = 100)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bit_count = length * 6;
                var byte_count = (bit_count + 7) / 8; // rounded up
                var bytes = new byte[byte_count];
                rng.GetBytes(bytes);
                return Base64Util.UrlEncode(bytes);
            }
        }
        private string GenerateNonce()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            var nonce = new char[100];
            for (int i = 0; i < nonce.Length; i++)
            {
                nonce[i] = chars[random.Next(chars.Length)];
            }
            return new string(nonce);
        }
    }

    public class Base64Util
  {
    internal const string WebEncoders_InvalidCountOffsetOrLength = "Invalid {0}, {1} or {2} length.";
    internal const string WebEncoders_MalformedInput = "Malformed input: {0} is an invalid input length.";

    public static string UrlEncode(byte[] input)
    {
      if (input == null)
      {
        throw new ArgumentNullException(nameof(input));
      }

      // Special-case empty input
      if (input.Length == 0)
      {
        return string.Empty;
      }

      var buffer = new char[GetArraySizeRequiredToEncode(input.Length)];
      var numBase64Chars = Convert.ToBase64CharArray(input, 0, input.Length, buffer, 0);

      // Fix up '+' -> '-' and '/' -> '_'. Drop padding characters.
      for (var i = 0; i < numBase64Chars; i++)
      {
        var ch = buffer[i];
        if (ch == '+')
        {
          buffer[i] = '-';
        }
        else if (ch == '/')
        {
          buffer[i] = '_';
        }
        else if (ch == '=')
        {
          return new string(buffer, startIndex: 0, length: i);
        }
      }

      return new string(buffer, startIndex: 0, length: numBase64Chars);
    }

    public static byte[] UrlDecode(string input)
    {
      var buffer = new char[GetArraySizeRequiredToDecode(input.Length)];
      if (input == null)
      {
        throw new ArgumentNullException(nameof(input));
      }

      // Assumption: input is base64url encoded without padding and contains no whitespace.

      var paddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(input.Length);
      var arraySizeRequired = checked(input.Length + paddingCharsToAdd);

      // Copy input into buffer, fixing up '-' -> '+' and '_' -> '/'.
      var i = 0;
      for (var j = 0; i < input.Length; i++, j++)
      {
        var ch = input[j];
        if (ch == '-')
        {
          buffer[i] = '+';
        }
        else if (ch == '_')
        {
          buffer[i] = '/';
        }
        else
        {
          buffer[i] = ch;
        }
      }

      // Add the padding characters back.
      for (; paddingCharsToAdd > 0; i++, paddingCharsToAdd--)
      {
        buffer[i] = '=';
      }

      // Decode.
      // If the caller provided invalid base64 chars, they'll be caught here.
      return Convert.FromBase64CharArray(buffer, 0, arraySizeRequired);
    }

    private static int GetArraySizeRequiredToEncode(int count)
    {
      var numWholeOrPartialInputBlocks = checked(count + 2) / 3;
      return checked(numWholeOrPartialInputBlocks * 4);
    }

    private static int GetArraySizeRequiredToDecode(int count)
    {
      if (count < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(count));
      }

      if (count == 0)
      {
        return 0;
      }

      var numPaddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(count);

      return checked(count + numPaddingCharsToAdd);
    }

    private static int GetNumBase64PaddingCharsToAddForDecode(int inputLength)
    {
      switch (inputLength % 4)
      {
        case 0:
          return 0;
        case 2:
          return 2;
        case 3:
          return 1;
        default:
          throw new FormatException(
              string.Format(
                  CultureInfo.CurrentCulture,
                  WebEncoders_MalformedInput,
                  inputLength));
      }
    }
  }

    public class AuthQueryParams
    {
        public string ClientId { get; set; }
        public string ResponseType { get; set; }
        public string RedirectUri { get; set; }
        public string CodeChallengeMethod { get; set; }
        public string CodeChallenge { get; set; }
        public string State { get; set; }
        public IReadOnlyList<string> Scopes { get; set; }

        public static AuthQueryParams CreatePKCEParams(string clientId, string redirectUri, string codeChallenge,
            IReadOnlyList<string> scopes, string state = null) => new AuthQueryParams
            {
                ClientId = clientId,
                ResponseType = "code",
                RedirectUri = redirectUri,
                CodeChallengeMethod = "S256",
                CodeChallenge = codeChallenge,
                State = state,
                Scopes = scopes
            };

        public string ToRequestUri()
        {
            var dashSeparatedScopes = string.Join(' ', Scopes);
            var uri =
                $"https://accounts.spotify.com/authorize?response_type={ResponseType}&client_id={ClientId}&redirect_uri={RedirectUri}&scope={dashSeparatedScopes}&state={State}&code_challenge={CodeChallenge}&code_challenge_method={CodeChallengeMethod}";

            return uri;
        }
    }

    public class ExchangeAuthorizationCodeResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
