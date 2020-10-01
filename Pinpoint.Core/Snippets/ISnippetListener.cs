using Pinpoint.Core.Snippets;

namespace Pinpoint.Core.Snippets
{
    public interface ISnippetListener
    {
        void SnippetAdded(object sender, ISnippet snippet);

        void SnippetRemoved(object sender, ISnippet snippet);
    }
}
