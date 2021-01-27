using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pinpoint.Plugin.Finance
{
    public class YahooFinanceApi
    {
        private const string Url = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?interval=2m&useYfid=true&range=1d";
        
        public YahooFinanceApi(TimeSpan cacheTimeout)
        {
            Repository = new YahooFinanceRepository(cacheTimeout);
        }

        public YahooFinanceRepository Repository { get; }
        
        public async Task<YahooFinanceResponse> Lookup(string ticker)
        {
            // Check if ticker is cached
            if (Repository.TryGet(ticker, out var response))
            {
                return response;
            }

            var httpResponse = string.Empty;
            try
            {
                using var httpClient = new HttpClient();
                httpResponse = await httpClient.GetStringAsync(string.Format(Url, ticker));
            }
            catch (HttpRequestException)
            {
                return null;
            }

            var result = JObject.Parse(httpResponse)["chart"]["result"].ToArray();
            if (result.Length == 0)
            {
                return null;
            }
            
            var meta = result[0]["meta"].ToString();
            response = JsonConvert.DeserializeObject<YahooFinanceResponse>(meta);

            // Don't consider non-equity instrument types
            if (!response.InstrumentType.Equals("EQUITY"))
            {
                return null;
            }

            // Cache ticker response
            Repository.Put(ticker, response);
            return response;
        }
    }
}