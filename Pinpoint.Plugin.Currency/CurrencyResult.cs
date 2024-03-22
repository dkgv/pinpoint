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
            var numDigits = 3;

            if (amount < 0.001)
            {
                var decimalPart = amount.ToString(CultureInfo.InvariantCulture).Split('.')[1];
                if (decimalPart.Length > numDigits)
                {
                    numDigits = decimalPart.Length;
                }
            }

            var numberFormat = new NumberFormatInfo
            {
                NumberDecimalDigits = numDigits
            };

            return $"{amount.ToString("N", numberFormat)} {currencyToISO4217.ToUpper()}";
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Coins;
    }
}
