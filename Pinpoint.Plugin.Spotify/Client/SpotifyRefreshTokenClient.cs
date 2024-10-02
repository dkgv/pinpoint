using PinPoint.Plugin.Spotify;

namespace Pinpoint.Plugin.Spotify.Client
{
    
    public interface ISpotifyRefreshTokenClient
    {
        string GetRefreshToken();
        void SaveRefreshToken(string refreshToken);
    }

    public class SpotifyRefreshTokenClient : ISpotifyRefreshTokenClient
    {
        private readonly SpotifyPlugin _spotifyPlugin;
        public SpotifyRefreshTokenClient(SpotifyPlugin spotifyPlugin)
        {
            _spotifyPlugin = spotifyPlugin;
        }

        public string GetRefreshToken()
        {
            var refreshToken = _spotifyPlugin.Storage.Internal["refresh_token"].ToString();
            
            return refreshToken;
        }

        public void SaveRefreshToken(string refreshToken)
        {
            _spotifyPlugin.Storage.Internal["refresh_token"] = refreshToken;
            _spotifyPlugin.Save();
        }
    }
}