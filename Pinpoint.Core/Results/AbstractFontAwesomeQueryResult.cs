using System.Drawing;
using FontAwesome5;

namespace Pinpoint.Core.Results
{
    public abstract class AbstractFontAwesomeQueryResult : AbstractQueryResult
    {
        protected AbstractFontAwesomeQueryResult()
        {
        }

        protected AbstractFontAwesomeQueryResult(string title, string subtitle = "") : base(title, subtitle)
        {
        }

        public abstract EFontAwesomeIcon FontAwesomeIcon { get; }

        public override Bitmap Icon => FontAwesomeBitmapRepository.Get(FontAwesomeIcon);
    }
}