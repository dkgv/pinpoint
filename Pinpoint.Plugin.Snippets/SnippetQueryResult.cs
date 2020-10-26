using FontAwesome5;

namespace Pinpoint.Plugin.Snippets
{
    public class SnippetQueryResult : AbstractQueryResult
    {
        private readonly AbstractSnippet _snippet;

        public SnippetQueryResult(AbstractSnippet snippet) : base(snippet.Identifier, snippet.FilePath)
        {
            _snippet = snippet;
        }

        public new object Instance => _snippet;

        public override EFontAwesomeIcon Icon => EFontAwesomeIcon.Solid_Code;

        public override void OnSelect()
        {
        }
    }
}
