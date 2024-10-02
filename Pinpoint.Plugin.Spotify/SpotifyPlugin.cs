using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Plugin;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifyPlugin : AbstractPlugin
    {
        private readonly HashSet<string> _keywords = new()
        {
            "album", "artist", "episode", "play", "playlist", "show", "skip", "next", "prev", "back", "pause"
        };
        private readonly AuthenticationManager _authManager = new();
        private SpotifyClient _spotifyClient;
        private bool _isAuthenticated;

        public override PluginManifest Manifest { get; } = new("Spotify Controller", PluginPriority.High)
        {
            Description = "Control any Spotify session without leaving your workflow. Requires sign-in on first use.\n\nExamples: \"album <name>\", \"artist <name>\", \"episode <name>\", \"play <name>\", \"playlist <name>\", \"show <name>\", \"skip\", \"next\", \"prev\", \"back\", \"pause\""
        };

        public override Task<bool> Initialize()
        {
            if (Storage.Internal.TryGetValue("refresh_token", out object value))
            {
                _isAuthenticated = !string.IsNullOrWhiteSpace(value.ToString());
            }

            _spotifyClient = new SpotifyClient(new SpotifyRefreshTokenClient(this));

            return Task.FromResult(_isAuthenticated);
        }

        public override async Task<bool> ShouldActivate(Query query)
        {
            var queryParts = query.Raw.Split(new[] { ' ' }, 2);

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
                            await _spotifyClient.InitializeClientWithTokens(tokens);
                            _isAuthenticated = true;
                        }

                        break;
                    }
            }

            return _isAuthenticated;
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var queryParts = query.Raw.Split(new[] { ' ' }, 2);

            if (queryParts[0] == "skip" || queryParts[0] == "next" ||
                queryParts[0] == "prev" || queryParts[0] == "back")
            {
                yield return new ChangeTrackResult(_spotifyClient, queryParts[0]);
                yield break;
            }

            yield return new PlayPauseResult(_spotifyClient);

            var isSearchQuery = queryParts.Length > 1 &&
                                queryParts[1].Length > 3 &&
                                queryParts[0] != "pause";

            if (!isSearchQuery)
            {
                yield break;
            }

            var queryTypes = MapToSpotifySearchType(queryParts[0]);
            var searchQuery = queryParts[1];

            var searchResults = await _spotifyClient.Search(searchQuery, queryTypes);
            if (searchResults == null || searchResults.Count == 0)
            {
                yield break;
            }

            foreach (var searchResult in searchResults)
            {
                yield return new SpotifySearchResult(_spotifyClient, searchResult.DisplayString, searchResult.Uri, searchResult.Type);
            }
        }

        private static string[] MapToSpotifySearchType(string type)
        {
            switch (type)
            {
                case "play":
                    return new[] { "track", "playlist" };

                default:
                    return new[] { type };
            }
        }
    }
}
