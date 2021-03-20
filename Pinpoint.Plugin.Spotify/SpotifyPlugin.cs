using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifyPlugin : IPlugin
    {
        private readonly HashSet<string> _keywords = new HashSet<string> {"album", "artist", "episode", "play", "playlist", "show", "skip", "next", "prev", "back", "pause"};
        private readonly AuthenticationManager _authManager = new AuthenticationManager();
        private readonly SpotifyClient _spotifyClient = SpotifyClient.GetInstance();
        private bool _isAuthenticated;

        public PluginMeta Meta { get; set; } = new PluginMeta("Spotify Controller");

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool TryLoad()
        {
            var settings = AppSettings.GetOrDefault("spotify", new SpotifyPluginSettings());

            _isAuthenticated = !string.IsNullOrWhiteSpace(settings.RefreshToken);

            return true;
        }

        public void Unload()
        {
            _spotifyClient.Dispose();
        }

        public async Task<bool> Activate(Query query)
        {
            var queryParts = query.RawQuery.Split(new[] {' '}, 2);

            var shouldActivate = queryParts.Length > 0 && _keywords.Contains(queryParts[0]);

            switch (shouldActivate)
            {
                case false:
                    return false;
                case true when !_isAuthenticated:
                {
                    var tokens = await _authManager.Authenticate();
                    if (tokens?.access_token != null && tokens.refresh_token != null)
                    {
                        _spotifyClient.InitializeClientWithTokens(tokens);
                        _isAuthenticated = true;
                    }

                    break;
                }
            }

            return _isAuthenticated;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var queryParts = query.RawQuery.Split(new[] { ' ' }, 2);

            if (queryParts[0] == "skip" || queryParts[0] == "next" ||
                queryParts[0] == "prev" || queryParts[0] == "back") 
            {
                yield return new ChangeTrackResult(queryParts[0]);
                yield break;
            }

            yield return new PlayPauseResult();

            var isSearchQuery = queryParts.Length > 1 &&
                                queryParts[1].Length > 3 && 
                                queryParts[0] != "pause";

            if(!isSearchQuery) yield break;

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
}
