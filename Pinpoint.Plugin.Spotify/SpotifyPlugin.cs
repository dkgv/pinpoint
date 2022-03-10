using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Spotify.Client;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifyPlugin : IPlugin
    {
        private readonly HashSet<string> _keywords = new()
        {
            "album", "artist", "episode", "play", "playlist", "show", "skip", "next", "prev", "back", "pause"
        };
        private readonly AuthenticationManager _authManager = new();
        private readonly SpotifyClient _spotifyClient = SpotifyClient.GetInstance();
        private bool _isAuthenticated;

        private const string Description = "Control Spotify without leaving your workflow. Requires sign-in on first use. Requires Spotify to be running on any device you are signed in to.\n\nExamples: \"album <name>\", \"artist <name>\", \"episode <name>\", \"play <name>\", \"playlist <name>\", \"show <name>\", \"skip\", \"next\", \"prev\", \"back\", \"pause\"";

        public PluginMeta Meta { get; set; } = new("Spotify Controller", Description, PluginPriority.Highest);

        public PluginStorage Storage { get; set; } = new();

        public bool IsLoaded { get; set; }

        public Task<bool> TryLoad()
        {
            if (Storage.InternalSettings.ContainsKey("refresh_token"))
            {
                _isAuthenticated = !string.IsNullOrWhiteSpace(Storage.InternalSettings["refresh_token"].ToString());
            }

            SpotifyClient.Plugin = this; // Dirty hack

            return Task.FromResult(IsLoaded = true);
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

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
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

            if (!isSearchQuery)
            {
                yield break;
            }

            var queryType = MapToSpotifySearchType(queryParts[0]);
            var searchQuery = queryParts[1];

            var searchResults = await _spotifyClient.Search(searchQuery, queryType);
            if (searchResults == null || searchResults.Count == 0)
            {
                yield break;
            }

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
