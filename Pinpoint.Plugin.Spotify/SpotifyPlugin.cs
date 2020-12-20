using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Plugin;
using System.Web;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifyPlugin : IPlugin
    {
        private readonly AuthenticationManager _authManager = new AuthenticationManager();
        private readonly SpotifyClient _spotifyClient = SpotifyClient.GetInstance();

        public PluginMeta Meta { get; set; } = new PluginMeta("Spotify Plugin");

        public void Load()
        {
           // Check auth here to ensure we always have a valid refresh token
           // Case 1: no refresh token, no access token - prompt login in browser
           // Case 2: refresh token, no access token -attempt to exchange refresh token, if that fails, prompt login in browser
           //Case 3: refresh token, access token - use access token, if response status code is 401, exchange refresh token and try again
           var tokens = _authManager.Authenticate();
           _spotifyClient.InitializeClientWithTokens(tokens);
        }

        public void Unload()
        {
            _spotifyClient.Dispose();
        }

        public async Task<bool> Activate(Query query)
        {
            return query.RawQuery.StartsWith("¤") && query.RawQuery.Length > 4;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var res = await _spotifyClient.Search(query.RawQuery.Substring(1));

            foreach (var trackResult in res)
            {
                yield return new SpotifySearchResult(trackResult.Name, trackResult.Uri);
            }
        }
    }
}
