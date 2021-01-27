using System;
using System.Collections.Generic;

namespace Pinpoint.Plugin.Finance
{
    public class YahooFinanceRepository
    {
        public YahooFinanceRepository(TimeSpan cacheTimeout)
        {
            CacheTimeout = cacheTimeout;
        }

        public TimeSpan CacheTimeout { get; set; }

        public Dictionary<string, YahooFinanceResponse> ResponseCache = new Dictionary<string, YahooFinanceResponse>();
        
        public Dictionary<string, long> UpdateTimeCache = new Dictionary<string, long>();

        public bool TryGet(string ticker, out YahooFinanceResponse response)
        {
            if (!ResponseCache.ContainsKey(ticker))
            {
                response = null;
                return false;
            }
            
            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (now - UpdateTimeCache[ticker] >= CacheTimeout.Milliseconds)
            {
                response = null;
                return false;
            }

            response = ResponseCache[ticker];
            return true;
        }

        public void Put(string ticker, YahooFinanceResponse response)
        {
            ResponseCache[ticker] = response;
            UpdateTimeCache[ticker] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}