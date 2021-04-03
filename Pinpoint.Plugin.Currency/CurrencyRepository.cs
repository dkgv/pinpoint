using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyRepository
    {
        private readonly HashSet<string> _validIsos = new HashSet<string>();

        public CurrencyRepository()
        {
            CacheFiatRatesEurBase();
            CacheCryptoRates();

            foreach (var (@base, model) in CurrencyModels)
            {
                _validIsos.Add(@base.ToLower());
                foreach (var (currency, _) in model.Rates)
                {
                    _validIsos.Add(currency.ToLower());
                }
            }
        }

        public bool IsIsoValid(string iso) => _validIsos.Contains(iso.ToLower());

        public Dictionary<string, CurrencyModel> CurrencyModels { get; } = new Dictionary<string, CurrencyModel>();

        // Convert all values for USD
        
        public double ConvertFromTo(string from, double value, string to)
        {
            from = from.ToUpper();
            to = to.ToUpper();

            // Check if we know rate
            if (CurrencyModels.ContainsKey(from) && CurrencyModels[from].Rates.ContainsKey(to))
            {
                return CurrencyModels[from].Rates[to] * value;
            }

            // Check if we know inverse rate
            if (CurrencyModels.ContainsKey(to) && CurrencyModels[to].Rates.ContainsKey(from))
            {
                var invertedRate = 1 / CurrencyModels[to].Rates[from];
                
                if (!CurrencyModels.ContainsKey(@from))
                {
                    CurrencyModels[from] = new CurrencyModel(from)
                    {
                        Rates = new Dictionary<string, dynamic>()
                    };
                }
                
                CurrencyModels[from].Rates[to] = invertedRate;
                return invertedRate * value;
            }
            
            // Convert from EUR
            var eurRates = CurrencyModels["EUR"].Rates;
            if (eurRates.ContainsKey(to))
            {
                var rate = eurRates[to] * value / eurRates[from];
                if (!CurrencyModels.ContainsKey(@from))
                {
                    CurrencyModels[from] = new CurrencyModel(from)
                    {
                        Rates = new Dictionary<string, dynamic>()
                    };
                }
                
                CurrencyModels[from].Rates[to] = rate;
                return rate;
            }

            return 1;
        }

        private void CacheFiatRatesEurBase()
        {
            // Load exchange rates
            var url = "http://getpinpoint.herokuapp.com/api/currency";
            try
            {
                using var client = new WebClient();
                var json = client.DownloadString(url);
                CurrencyModels["EUR"] = JsonConvert.DeserializeObject<CurrencyModel>(json);
            }
            catch (WebException)
            {
                // Likely invalid currency
            }
        }

        private void CacheCryptoRates()
        {
            CurrencyModel PopulateModel(string ticker, double usdPrice)
            {
                var eurPrice = ConvertFromTo("USD", usdPrice, "EUR");

                var model = new CurrencyModel(ticker)
                {
                    Rates = new Dictionary<string, dynamic> { ["EUR"] = eurPrice }
                };

                foreach (var @base in CurrencyModels["EUR"].Rates.Keys.Where(@base => !@base.Equals("EUR")))
                {
                    model.Rates[@base] = ConvertFromTo("EUR", usdPrice, @base);
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
    }
}