using System;
using System.Collections.Generic;

namespace Pinpoint.Plugin.Finance
{
    public class PriceRepository
    {
        public PriceRepository(TimeSpan cacheTimeout)
        {
            CacheTimeout = cacheTimeout;
        }

        public TimeSpan CacheTimeout { get; set; }

        public Dictionary<string, YahooFinancePriceResponse> ResponseCache = new Dictionary<string, YahooFinancePriceResponse>();
        
        public Dictionary<string, long> UpdateTimeCache = new Dictionary<string, long>();

        public bool TryGet(string ticker, out YahooFinancePriceResponse priceResponse)
        {
            if (!ResponseCache.ContainsKey(ticker))
            {
                priceResponse = null;
                return false;
            }
            
            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (now - UpdateTimeCache[ticker] >= CacheTimeout.Milliseconds)
            {
                priceResponse = null;
                return false;
            }

            priceResponse = ResponseCache[ticker];
            return true;
        }

        public void Put(string ticker, YahooFinancePriceResponse priceResponse)
        {
            ResponseCache[ticker] = priceResponse;
            UpdateTimeCache[ticker] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}