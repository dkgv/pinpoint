using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.PasswordGenerator
{
    public class PasswordErrorResult: AbstractFontAwesomeQueryResult
    {
        public PasswordErrorResult(): base("The length specified is invalid")
        {
            Subtitle = "Has to be a number between 1 and 100.";
        }
        public override void OnSelect() { }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Key;
    }
}