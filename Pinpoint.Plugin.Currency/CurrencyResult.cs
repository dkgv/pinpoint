using System.Globalization;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyResult : CopyabableQueryOption
    {
        public CurrencyResult(double value, string to) : base(FormatValue(value, to), value.ToString())
        {
        }

        private static string FormatValue(double amount, string currencyToISO4217)
        {
            var numberFormat = CultureInfo.CurrentCulture.NumberFormat;
            var formattedAmount = amount.ToString("N", numberFormat);

            currencyToISO4217 = currencyToISO4217.ToUpper();

            return $"{formattedAmount} {currencyToISO4217}";
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Coins;
    }
}
