using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifyPlugin : IPlugin
    {
        private readonly string[] _prefixes = {"album", "artist", "episode", "play", "playlist", "show"};
        private readonly AuthenticationManager _authManager = new AuthenticationManager();
        private readonly SpotifyClient _spotifyClient = SpotifyClient.GetInstance();

        public PluginMeta Meta { get; set; } = new PluginMeta("Spotify Controller");

        public void Load()
        {
            var settings = AppSettings.GetAsOrDefault<SpotifyPluginSettings>("spotify", null);
            if(settings?.RefreshToken == null)
            {
                var tokens = _authManager.Authenticate();
                _spotifyClient.InitializeClientWithTokens(tokens);
            }
        }

        public void Unload()
        {
            _spotifyClient.Dispose();
        }

        public Task<bool> Activate(Query query)
        {
            var queryParts = query.RawQuery.Split(new[] {' '}, 2);
            var shouldActivate = queryParts.Length > 1 && _prefixes.Any(prefix => queryParts[0] == prefix) &&
                                 queryParts[1].Length > 3;
            return Task.FromResult(shouldActivate);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var queryParts = query.RawQuery.Split(new[] { ' ' }, 2);
            var queryType = MapToSpotifySearchType(queryParts[0]);
            var searchQuery = queryParts[1];

            var searchResults = await _spotifyClient.Search(searchQuery, queryType);

            foreach (var searchResult in searchResults)
            {
                yield return new SpotifySearchResult(searchResult.DisplayString, searchResult.Uri);
            }
        }

        private static string MapToSpotifySearchType(string type)
        {
            return type == "play" ? "track" : type;
        }
    }

    public class PlayPauseResult: AbstractFontAwesomeQueryResult
    {
        public override void OnSelect()
        {
            throw new NotImplementedException();
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Play;
    }
}
