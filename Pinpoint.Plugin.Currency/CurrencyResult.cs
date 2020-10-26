using System.Globalization;
using FontAwesome5;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyResult : AbstractQueryResult
    {
        public CurrencyResult(double value, string to) : base("= " + value.ToString(CultureInfo.InvariantCulture) + " " + to.ToUpper())
        {
        }

        public override EFontAwesomeIcon Icon => EFontAwesomeIcon.Solid_Coins;

        public override void OnSelect()
        {
        }
    }
}
