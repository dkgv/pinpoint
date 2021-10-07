using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;

namespace PinPoint.Plugin.Spotify
{
    public class AuthQueryParams
    {
        public string ClientId { get; set; }
        public string ResponseType { get; set; }
        public string RedirectUri { get; set; }
        public string CodeChallengeMethod { get; set; }
        public string CodeChallenge { get; set; }
        public string State { get; set; }
        public IReadOnlyList<string> Scopes { get; set; }

        public static AuthQueryParams CreatePkceParams(string clientId, string redirectUri, string codeChallenge,
            IReadOnlyList<string> scopes, string state = null) => new()
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
            var separatedScopes = string.Join(' ', Scopes);
            var parameters = new Dictionary<string, string>
            {
                {"client_id", ClientId},
                {"response_type", "code"},
                {"redirect_uri", "http://localhost:5001/SpotifyResponse"},
                {"code_challenge_method", "S256"},
                {"code_challenge", CodeChallenge},
                {"state", "1234567890"},
                {"scope", separatedScopes}
            };

            var url = QueryHelpers.AddQueryString("https://accounts.spotify.com/authorize", parameters);

            return url;
        }
    }
}