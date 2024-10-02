using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class UnauthenticatedResult : AbstractFontAwesomeQueryResult
    {
        private readonly AuthenticationManager _authenticationManager;
        private readonly SpotifyClient _spotifyClient;

        public UnauthenticatedResult(AuthenticationManager authenticationManager, SpotifyClient spotifyClient)
        {
            _authenticationManager = authenticationManager;    
            _spotifyClient = spotifyClient;
        }

        public override EFontAwesomeIcon FontAwesomeIcon => throw new System.NotImplementedException();

        public override void OnSelect()
        {
            Task.Run(async () => {
                var tokens = await _authenticationManager.Authenticate();
                if (tokens?.access_token != null && tokens.refresh_token != null)
                {
                    await _spotifyClient.InitializeClientWithTokens(tokens);
                }
            });
        }
    }
}