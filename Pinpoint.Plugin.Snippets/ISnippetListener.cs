namespace Pinpoint.Plugin.Snippets
{
    public interface ISnippetListener
    {
        public void SnippetAdded(object sender, SnippetsPlugin plugin, AbstractSnippet target);

        public void SnippetRemoved(object sender, SnippetsPlugin plugin, AbstractSnippet target);
    }
}
