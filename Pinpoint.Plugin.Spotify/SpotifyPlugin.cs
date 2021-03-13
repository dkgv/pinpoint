﻿using System;
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
        private readonly HashSet<string> _keywords = new HashSet<string> {"album", "artist", "episode", "play", "playlist", "show", "skip", "next", "prev", "back"};
        private readonly AuthenticationManager _authManager = new AuthenticationManager();
        private readonly SpotifyClient _spotifyClient = SpotifyClient.GetInstance();
        private readonly List<AbstractQueryResult> _defaultResults = new List<AbstractQueryResult>
        {
            new PausePlayResult()
        };

        public PluginMeta Meta { get; set; } = new PluginMeta("Spotify Controller");

        public bool TryLoad()
        {
            var settings = AppSettings.GetAsOrDefault<SpotifyPluginSettings>("spotify", null);

            if (settings?.RefreshToken != null) return true;

            var tokens = _authManager.Authenticate();
            _spotifyClient.InitializeClientWithTokens(tokens);
            return true;
        }

        public void Unload()
        {
            _spotifyClient.Dispose();
        }

        public Task<bool> Activate(Query query)
        {
            var queryParts = query.RawQuery.Split(new[] {' '}, 2);
            if (queryParts.Length == 0) return Task.FromResult(false);

            var matchesAnyKeyword = _keywords.Contains(queryParts[0]);

            var isSkipOrPreviousTrackQuery = queryParts[0] == "skip" || queryParts[0] == "next" ||
                                             queryParts[0] == "prev" || queryParts[0] == "back";
            var isSearchQuery = queryParts.Length > 1 &&
                                queryParts[1].Length > 3;

            var shouldActivate = matchesAnyKeyword && (isSkipOrPreviousTrackQuery || isSearchQuery);

            return Task.FromResult(shouldActivate);
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
