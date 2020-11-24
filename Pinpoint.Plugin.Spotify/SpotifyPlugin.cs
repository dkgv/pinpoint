using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Pinpoint.Plugin;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifyPlugin : IPlugin
    {
        public PluginMeta Meta { get; set; }
        public void Load()
        {
            //Check auth here to ensure we always have a valid refresh token
            //Case 1: no refresh token, no access token - prompt login in browser
            //Case 2: refresh token, no access token - attempt to exchange refresh token, if that fails, prompt login in browser
            //Case 3: refresh token, access token - use access token, if response status code is 401, exchange refresh token and try again
            throw new NotImplementedException();
        }

        public void Unload()
        {
            //Not much to do here
            throw new NotImplementedException();
        }

        public Task<bool> Activate(Query query)
        {
            //Need some way to distinguish between play/queue/next actions
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            //Search results go here I guess
            throw new NotImplementedException();
        }
    }

    public class SpotifyClient
    {

    }

    public class AuthenticationManager
    {
        private string GenerateNonce()
        {
            var buffer = new byte[32];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);

            return Convert.ToBase64String(buffer);
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
            var dashSeparatedScopes = string.Join('-', Scopes);
            var uri =
                $"https://accounts.spotify.com/authorize?response_type={ResponseType}&client_id={ClientId}&redirect_uri={RedirectUri}&scope={dashSeparatedScopes}&state={State}&code_challenge={CodeChallenge}&code_challenge_method={CodeChallengeMethod}";

            return uri;
        }
    }
}
