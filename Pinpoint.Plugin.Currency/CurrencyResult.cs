using System.Globalization;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Currency
{
    public class CurrencyResult : CopyabableQueryOption
    {
        public CurrencyResult(double value, string to) : base("= " + value.ToString(CultureInfo.InvariantCulture) + " " + to.ToUpper(), value.ToString())
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Coins;

        public override void OnSelect()
        {
        }
    }
}
