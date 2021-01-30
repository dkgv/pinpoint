using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyPlugin : IPlugin
    {
        private string _baseCurrency;
        private const string Symbols = "$£€¥";
        private static readonly Dictionary<string, CurrencyModel> CurrencyModels = new Dictionary<string, CurrencyModel>();

        public PluginMeta Meta { get; set; } = new PluginMeta("Currency Converter", PluginPriority.Highest);

        public void Load()
        {
            try
            {
                // Find base currency
                var ri = new RegionInfo(CultureInfo.CurrentCulture.Name);
                _baseCurrency = ri.ISOCurrencySymbol;
            }
            catch (ArgumentException)
            {
                _baseCurrency = "USD";
            }

            CacheFiatRates(_baseCurrency);
            CacheCryptoRates();
        }

        private void CacheFiatRates(string from)
        {
            // Load exchange rates
            var url = $"https://api.exchangeratesapi.io/latest?base={from}";
            try
            {
                using var client = new WebClient();
                var json = client.DownloadString(url);
                CurrencyModels[from] = JsonConvert.DeserializeObject<CurrencyModel>(json);
            }
            catch (WebException)
            {
                // Likely invalid currency
            }
        }

        private void CacheCryptoRates()
        {
            // Ensure USD rates are cached
            CacheFiatRates("USD");

            CurrencyModel PopulateModel(string ticker, double usdPrice)
            {
                var model = new CurrencyModel(ticker)
                {
                    Rates = new Dictionary<string, dynamic> {["USD"] = usdPrice}
                };

                foreach (var @base in CurrencyModels["USD"].Rates.Keys.Where(@base => !@base.Equals("USD")))
                {
                    model.Rates[@base] = ConvertFromTo("USD", usdPrice, @base);
                }

                return model;
            }
            
            const string url = "https://coinmarketcap.com/";
            var doc = new HtmlWeb().Load(url);
            
            const string xpath = "/html/body/div[1]/div/div[2]/div/div/div[2]/table";
            var table = doc.DocumentNode.SelectSingleNode(xpath);
            
            var rows = table.Descendants("tr").ToArray();
            for (var i = 1; i < rows.Length; i++)
            {
                var cols = rows[i].Descendants("td").ToArray();
                if (cols.Length != 11)
                {
                    continue;
                }
                
                var ps = cols[2].Descendants("p").ToArray();
                if (ps.Length != 2)
                {
                    continue;
                }
                
                // Extract current price for entry
                var priceNodeText = cols[3].InnerText;
                var priceText = priceNodeText.TrimStart('$').Replace(",", "");
                var price = double.Parse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture);

                // Skip stable-coins like USDT
                if (Math.Abs(price - 1) > 0.1)
                {
                    var ticker = cols[2].Descendants("p").ToArray()[1].InnerText;
                    CurrencyModels[ticker] = PopulateModel(ticker, price);
                }
            }
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

            // Matches 10 usd
            var hasNumber = query.Parts[0].All(IsNumber);
            if (hasNumber && query.Parts.Length == 2 && query.Parts[1].Length == 3)
            {
                return true;
            }

            // Matches inputs like 104.3 usd to/in eur
            var isConverting = raw.Contains("to") || raw.Contains("in");
            var correctParts = query.Parts.Length == 4 && query.Parts[3].Length == 3;

            return hasNumber && isConverting && correctParts;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
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
            
            // Is the `from` currency new?
            if (!CurrencyModels.ContainsKey(from))
            {
                CacheFiatRates(from);
            }

            var fromModel = CurrencyModels[from];
            
            // Check if we already know rate for `to`
            if (fromModel.Rates.ContainsKey(to))
            {
                return fromModel.Rates[to] * value;
            }

            // Check if we know inverse rate
            CurrencyModel toModel;
            if (CurrencyModels.ContainsKey(to) && (toModel = CurrencyModels[to]).Rates.ContainsKey(from))
            {
                fromModel.Rates[to] = 1 / toModel.Rates[from];
            }

            return fromModel.Rates.ContainsKey(to) 
                ? fromModel.Rates[to] * value 
                : 1;
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
