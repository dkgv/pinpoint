using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyPlugin : IPlugin
    {
        private CurrencyRepository _currencyRepo = new();
        private const string KeyBaseCurrency = "Base currency";
        private const string Symbols = "$£€¥";

        public PluginManifest Manifest { get; set; } = new("Currency Converter", PluginPriority.High)
        {
            Description = "Convert between currencies and cryptocurrencies.\n\nExamples: \"5 usd to jpy\", \"1 btc to cad\", \"1 eth to btc\""
        };

        public PluginStorage Storage { get; set; } = new()
        {
            User = UserSettings.Default(
                new Dictionary<string, object>{
                    {KeyBaseCurrency, GetDefaultBaseCurrency()}
                }
            )
        };

        private static string GetDefaultBaseCurrency()
        {
            string baseCurrency;
            try
            {
                // Find base currency
                var ri = new RegionInfo(CultureInfo.CurrentCulture.Name);
                baseCurrency = ri.ISOCurrencySymbol;
            }
            catch (ArgumentException)
            {
                baseCurrency = "USD";
            }
            return baseCurrency;
        }

        public async Task<bool> TryLoad()
        {
            await _currencyRepo.LoadCurrenciesInitial();
            return true;
        }

        // TODO rewrite into oblivion
        public async Task<bool> Activate(Query query)
        {
            var raw = query.RawQuery;

            if (raw.Length < 2)
            {
                return false;
            }

            bool IsNumber(char ch)
            {
                return char.IsDigit(ch) || ch == '.';
            }

            // Matches inputs like $100, €5
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

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var from = IdentifyFrom(query);
            var value = IdentifyValue(query);
            var to = IdentifyTo(query);

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                yield break;
            }

            var conversion = Math.Round(_currencyRepo.ConvertFromTo(from, value, to), 5);
            yield return new CurrencyResult(conversion, to);
        }

        private double IdentifyValue(Query query)
        {
            var match = Regex.Match(query.RawQuery, @"[+-]?([0-9]*[.])?[0-9]+");
            return double.Parse(match.Groups[0].Value, CultureInfo.InvariantCulture);
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
            var iso = query.Parts[1];
            return _currencyRepo.IsIsoValid(iso) ? iso : string.Empty;
        }

        private string IdentifyTo(Query query)
        {
            // Handles queries like 100 usd, 100 eur
            if (!query.RawQuery.Contains("in") && !query.RawQuery.Contains("to"))
            {
                return Storage.User.Str(KeyBaseCurrency);
            }

            // Get last part
            var iso = SymbolToISO(query.Parts[^1]);
            return _currencyRepo.IsIsoValid(iso) ? iso : string.Empty;
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
