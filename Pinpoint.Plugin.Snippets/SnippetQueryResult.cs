using FontAwesome5;
using Pinpoint.Core;

namespace Pinpoint.Plugin.Snippets
{
    public class SnippetQueryResult : AbstractFontAwesomeQueryResult
    {
        private readonly AbstractSnippet _snippet;

        public SnippetQueryResult(AbstractSnippet snippet) : base(snippet.Identifier, snippet.FilePath)
        {
            _snippet = snippet;
        }

        public new object Instance => _snippet;

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Code;

        public override void OnSelect()
        {
        }
    }
}
