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

        public CurrencyRepository(string baseCurrency)
        {
            CacheCryptoRates();
            CacheFiatRates(baseCurrency);

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

        public double ConvertFromTo(string from, double value, string to)
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
                    Rates = new Dictionary<string, dynamic> { ["USD"] = usdPrice }
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
    }
}