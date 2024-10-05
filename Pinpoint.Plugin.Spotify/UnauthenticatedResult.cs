using System;
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
        private readonly Action _onAuthenticated;

        public UnauthenticatedResult(AuthenticationManager authenticationManager, SpotifyClient spotifyClient, Action onAuthenticated) : base("Not signed in to Spotify", "Select this option to sign in.")
        {
            _authenticationManager = authenticationManager;
            _spotifyClient = spotifyClient;
            _onAuthenticated = onAuthenticated;
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;

        public override void OnSelect()
        {
            Task.Run(async () => {
                var tokens = await _authenticationManager.Authenticate();
                if (tokens?.access_token != null && tokens.refresh_token != null)
                {
                    await _spotifyClient.InitializeClientWithTokens(tokens);
                    _onAuthenticated();
                }
            });
        }
    }
}