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
        private readonly string[] _keywords = {"album", "artist", "episode", "play", "playlist", "show", "skip", "prev"};
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

            var matchesAnyKeyword = _keywords.Any(prefix => queryParts[0] == prefix);

            var isSkipOrPreviousTrackQuery = queryParts[0] == "skip" || queryParts[0] == "prev";
            var isSearchQuery = queryParts.Length > 1 &&
                                queryParts[1].Length > 3;

            var shouldActivate = matchesAnyKeyword && (isSkipOrPreviousTrackQuery || isSearchQuery);

            //var shouldActivate = queryParts.Length > 1 && _keywords.Any(prefix => queryParts[0] == prefix) &&
            //                     queryParts[1].Length > 3;
            return Task.FromResult(shouldActivate);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            yield return new PlayPauseResult();

            var queryParts = query.RawQuery.Split(new[] { ' ' }, 2);

            if (queryParts[0] == "skip" || queryParts[0] == "prev")
            {
                yield return new SkipTrackResult(queryParts[0]);
                yield break;
            }

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
        public PlayPauseResult(): base("Play/pause current track") { }
        public override void OnSelect()
        {
            SpotifyClient.GetInstance().PlayPauseCurrentTrack();
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;
    }

    public class SkipTrackResult: AbstractFontAwesomeQueryResult
    {
        private readonly string _keyword;

        public SkipTrackResult(string keyword): base(keyword == "skip" ? "Next track" : "Previous track")
        {
            _keyword = keyword;
        }
        public override void OnSelect()
        {
            if (_keyword == "skip")
            {
                SpotifyClient.GetInstance().NextTrack();
            }
            else
            {
                SpotifyClient.GetInstance().PreviousTrack();
            }
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Brands_Spotify;
    }
}
