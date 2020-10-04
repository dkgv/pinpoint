using Pinpoint.Core.Snippets;

namespace Pinpoint.Core.Snippets
{
    public interface ISnippetListener
    {
        void SnippetAdded(object sender, AbstractSnippet abstractSnippet);

        void SnippetRemoved(object sender, AbstractSnippet abstractSnippet);
    }
}
