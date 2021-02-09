using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pinpoint.Plugin.Finance
{
    public class YahooFinanceApi
    {
        private const string PriceUrl = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?interval=2m&useYfid=true&range=1d";
        private const string SearchUrl = "https://query2.finance.yahoo.com/v1/finance/search?q={0}&quotesCount=1&enableFuzzyQuery=false&newsCount=0";
        private readonly Dictionary<string, YahooFinanceSearchResponse> _searchCache = new Dictionary<string, YahooFinanceSearchResponse>();

        public YahooFinanceApi(TimeSpan cacheTimeout)
        {
            PriceRepository = new PriceRepository(cacheTimeout);
        }

        public PriceRepository PriceRepository { get; }

        public async Task<YahooFinanceSearchResponse> Search(string ticker)
        {
            if (_searchCache.ContainsKey(ticker))
            {
                return _searchCache[ticker];
            }

            var httpResponse = await SendGet(string.Format(SearchUrl, ticker));
            if (string.IsNullOrEmpty(httpResponse))
            {
                return null;
            }

            var quotes = JObject.Parse(httpResponse)["quotes"].ToArray();
            if (quotes.Length == 0)
            {
                return null;
            }

            var searchResponse = JsonConvert.DeserializeObject<YahooFinanceSearchResponse>(quotes[0].ToString());
            return _searchCache[ticker] = searchResponse;
        }

        public async Task<YahooFinancePriceResponse> LookupPrice(string ticker)
        {
            // Check if ticker is cached
            if (PriceRepository.TryGet(ticker, out var response))
            {
                return response;
            }

            var httpResponse = await SendGet(string.Format(PriceUrl, ticker));
            if (string.IsNullOrEmpty(httpResponse))
            {
                return null;
            }

            var result = JObject.Parse(httpResponse)["chart"]["result"].ToArray();
            if (result.Length == 0)
            {
                return null;
            }
            
            var meta = result[0]["meta"].ToString();
            response = JsonConvert.DeserializeObject<YahooFinancePriceResponse>(meta);

            // Don't consider non-equity instrument types
            if (!response.InstrumentType.Equals("EQUITY"))
            {
                return null;
            }

            // Cache ticker response
            PriceRepository.Put(ticker, response);
            return response;
        }

        private async Task<string> SendGet(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                return await httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}