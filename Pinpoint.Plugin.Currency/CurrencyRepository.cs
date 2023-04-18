using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyRepository
    {
        private readonly HashSet<string> _validIsos = new();

        public async Task LoadCurrenciesInitial()
        {
            await CacheRatesWithEurBase().ConfigureAwait(false);

            foreach (var (@base, model) in CurrencyModels)
            {
                if (model == null)
                {
                    continue;
                }

                _validIsos.Add(@base.ToLower());
                foreach (var (currency, _) in model.Rates)
                {
                    _validIsos.Add(currency.ToLower());
                }
            }
        }

        public bool IsIsoValid(string iso) => _validIsos.Contains(iso.ToLower());

        public Dictionary<string, CurrencyModel> CurrencyModels { get; } = new();

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
                
                if (!CurrencyModels.ContainsKey(from))
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
                var rate = eurRates[to] / eurRates[from];
                if (!CurrencyModels.ContainsKey(from))
                {
                    CurrencyModels[from] = new CurrencyModel(from)
                    {
                        Rates = new Dictionary<string, dynamic>()
                    };
                }
                
                CurrencyModels[from].Rates[to] = rate;
                return rate * value;
            }

            return 1;
        }

        private async Task CacheRatesWithEurBase()
        {
            // Load exchange rates
            var url = "https://usepinpoint.com/api/currency";
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url).ConfigureAwait(false);
                var json = await response.Content.ReadAsStringAsync();
                CurrencyModels["EUR"] = JsonConvert.DeserializeObject<CurrencyModel>(json);
            }
            catch (WebException)
            {
                // Likely invalid currency
            }
        }
    }
}