using FontAwesome5;
using Pinpoint.Core;

namespace Pinpoint.Plugin.Dictionary
{
    public class DictionaryResult : UrlQueryResult
    {
        public DictionaryResult(string definition, string url) : base(url)
        {
            Title = definition;
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Book;
    }
}