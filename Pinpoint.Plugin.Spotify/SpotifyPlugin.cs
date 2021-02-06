﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace PinPoint.Plugin.Spotify
{
    public class SpotifyPlugin : IPlugin
    {
        private readonly AuthenticationManager _authManager = new AuthenticationManager();
        private readonly SpotifyClient _spotifyClient = SpotifyClient.GetInstance();
        private readonly List<AbstractQueryResult> _defaultResults = new List<AbstractQueryResult>
        {
            new PausePlayResult()
        };

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

            return Task.FromResult(query.RawQuery.StartsWith("play") && query.RawQuery.Length > 4);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {

            var searchResults = await _spotifyClient.Search(query.RawQuery.Substring(4));

            foreach (var trackResult in searchResults)
            {
                var result = trackResult.Name;
                if(trackResult.Artists.Count > 0)
                {
                    var artistsString = string.Join(", ", trackResult.Artists.Select(a => a.Name));
                    result = $"{result} - {artistsString}";
                }

                yield return new SpotifySearchResult(result, trackResult.Uri);
            }
        }
    }
}
