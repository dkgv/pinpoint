using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyPlugin : IPlugin
    {
        private string _baseCurrency;
        private const string Symbols = "$£€¥";
        private static readonly Dictionary<string, CurrencyModel> CurrencyModels = new Dictionary<string, CurrencyModel>();

        public PluginMeta Meta { get; set; } = new PluginMeta("Currency Converter", true);

        public void Load()
        {
            // Find base currency
            var ri = new RegionInfo(Thread.CurrentThread.CurrentUICulture.LCID);
            _baseCurrency = ri.ISOCurrencySymbol;

            // Cache rates for base to allow "€5" type queries
            CacheRates(_baseCurrency);
        }

        private void CacheRates(string from)
        {
            // Load exchange rates
            var url = $"https://api.exchangeratesapi.io/latest?base={from}";
            using var client = new WebClient();
            var json = client.DownloadString(url);
            CurrencyModels[from] = JsonConvert.DeserializeObject<CurrencyModel>(json);
        }

        public void Unload()
        {
        }

        // TODO rewrite into oblivion
        public async Task<bool> Activate(Query query)
        {
            var raw = query.RawQuery;

            bool IsNumber(char ch)
            {
                return char.IsDigit(ch) || ch == '.';
            }

            // Matches inputs like $100 or €5
            var hasSymbol = raw.Length > 1 && Symbols.Contains(query.Prefix());
            if (hasSymbol && raw.Substring(1).All(IsNumber))
            {
                return true;
            }

            // Matches inputs like $5 to/in eur
            if (hasSymbol && query.Parts.Length == 3 && "to in".Contains(query.Parts[1]) && query.Parts[2].Length == 3)
            {
                return true;
            }

            // Matches inputs like 104.3 usd to/in eur
            var hasNumber = query.Parts[0].All(IsNumber);
            var isConverting = raw.Contains("to") || raw.Contains("in");
            var correctParts = query.Parts.Length == 4 && query.Parts[3].Length == 3;

            return hasNumber && isConverting && correctParts;
        }

        public async IAsyncEnumerable<IQueryResult> Process(Query query)
        {
            var from = IdentifyFrom(query);
            var value = IdentifyValue(query);
            var to = IdentifyTo(query);
            var conversion = Math.Round(ConvertFromTo(from, value, to), 5);

            yield return new CurrencyResult(conversion, to);
        }

        private double ConvertFromTo(string from, double value, string to)
        {
            from = from.ToUpper();
            to = to.ToUpper();

            if (!CurrencyModels.ContainsKey(from))
            {
                CacheRates(from);
            }

            return CurrencyModels[from].Rates[to] * value;
        }

        private double IdentifyValue(Query query)
        {
            var match = Regex.Match(query.RawQuery, @"[+-]?(\d*\.)?\d+");
            return double.Parse(match.Groups[0].Value);
        }

        private string IdentifyFrom(Query query)
        {
            // Handles $100
            if (Symbols.Contains(query.Prefix()))
            {
                return SymbolToISO(query.Prefix());
            }

            // Handles $ 100
            var part = query.Parts[0];
            var maybeISO = SymbolToISO(part);
            if (!part.Equals(maybeISO))
            {
                return maybeISO;
            }

            // Handles 100 usd
            return query.Parts[1];
        }

        private string IdentifyTo(Query query)
        {
            if (!query.RawQuery.Contains("in") && !query.RawQuery.Contains("to"))
            {
                return _baseCurrency;
            }

            return SymbolToISO(query.Parts[^1]);
        }

        private string SymbolToISO(string symbol)
        {
            return symbol switch
            {
                "$" => "USD",
                "£" => "GBP",
                "€" => "EUR",
                "¥" => "YEN",
                _ => symbol
            };
        }
    }
}
