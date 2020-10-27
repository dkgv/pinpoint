using System.Globalization;
using FontAwesome5;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyResult : AbstractFontAwesomeQueryResult
    {
        public CurrencyResult(double value, string to) : base("= " + value.ToString(CultureInfo.InvariantCulture) + " " + to.ToUpper())
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Coins;

        public override void OnSelect()
        {
        }
    }
}
