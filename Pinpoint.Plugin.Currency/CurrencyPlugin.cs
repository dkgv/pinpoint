using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyPlugin : AbstractPlugin
    {
        private CurrencyRepository _currencyRepo = new();
        private const string KeyBaseCurrency = "Base currency";
        private const string Symbols = "$£€¥";

        public override PluginManifest Manifest { get; } = new("Currency Converter", PluginPriority.High)
        {
            Description = "Convert between currencies and cryptocurrencies.\n\nExamples: \"5 usd to jpy\", \"1 btc to cad\", \"1 eth to btc\""
        };

        public override PluginState State { get; } = new()
        {
            HasModifiableSettings = true
        };

        public override PluginStorage Storage { get; } = new()
        {
            User = new UserSettings { { KeyBaseCurrency, GetDefaultBaseCurrency() } }
        };

        private static string GetDefaultBaseCurrency()
        {
            string baseCurrency;
            try
            {
                var ri = new RegionInfo(CultureInfo.CurrentCulture.Name);
                baseCurrency = ri.ISOCurrencySymbol;
            }
            catch (ArgumentException)
            {
                baseCurrency = "USD";
            }
            return baseCurrency;
        }

        public override async Task<bool> Initialize()
        {
            await _currencyRepo.LoadCurrenciesInitial();
            return true;
        }

        // TODO rewrite into oblivion
        public override async Task<bool> ShouldActivate(Query query)
        {
            var raw = query.Raw;
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

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var from = IdentifyFrom(query).ToUpper();
            var value = IdentifyValue(query);
            var to = IdentifyTo(query).ToUpper();

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || from.Equals(to))
            {
                yield break;
            }

            var conversion = Math.Round(_currencyRepo.ConvertFromTo(from, value, to), 10);
            yield return new CurrencyResult(conversion, to);
        }

        private double IdentifyValue(Query query)
        {
            var match = Regex.Match(query.Raw, @"[+-]?([0-9]*[.])?[0-9]+");
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
            if (!query.Raw.Contains("in") && !query.Raw.Contains("to"))
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
