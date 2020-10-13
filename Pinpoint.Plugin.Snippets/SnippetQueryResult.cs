namespace Pinpoint.Plugin.Snippets
{
    public class SnippetQueryResult : IQueryResult
    {
        private readonly AbstractSnippet _snippet;

        public SnippetQueryResult(AbstractSnippet snippet)
        {
            _snippet = snippet;
            Title = snippet.Identifier;
            Subtitle = snippet.FilePath;
        }

        public string Title { get; }

        public string Subtitle { get; }

        public object Instance => _snippet;

        public void OnSelect()
        {
        }
    }
}
