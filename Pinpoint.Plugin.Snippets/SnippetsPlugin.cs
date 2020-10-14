using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Snippets
{
    public class SnippetsPlugin : IPlugin
    {
        public static readonly string FileSnippetsKey = "file_snippets";
        public static readonly string TextSnippetsKey = "text_snippets";
        public static readonly string OcrSnippetsKey = "ocr_snippets";

        public SnippetsPlugin(ISnippetListener settingsWindow)
        {
            Listeners.Add(settingsWindow);
        }

        public List<AbstractSnippet> Snippets { get; } = new List<AbstractSnippet>();

        public List<ISnippetListener> Listeners { get; } = new List<ISnippetListener>();

        public PluginMeta Meta { get; set; } = new PluginMeta("Snippets", true);

        public void Load()
        {
            void LoadSnippets<T>(string key) where T : AbstractSnippet
            {
                if (AppSettings.Contains(key))
                {
                    var sources = AppSettings.GetListAs<T>(key);
                    foreach (var snippet in sources)
                    {
                        AddSnippet(this, snippet);
                    }
                }
            }

            LoadSnippets<FileSnippet>(FileSnippetsKey);
            LoadSnippets<TextSnippet>(TextSnippetsKey);
            LoadSnippets<OcrTextSnippet>(OcrSnippetsKey);
        }

        public bool AddSnippet(object sender, AbstractSnippet snippet)
        {
            // Prevents duplicate sources
            if (Snippets.Contains(snippet))
            {
                return false;
            }

            Snippets.Add(snippet);

            // Update settings to include newly added snippet
            var snippetsOfType = Snippets.Where(s => s.GetType() == snippet.GetType());
            AppSettings.PutAndSave(GetKey(snippet), snippetsOfType);

            // Notify listeners that a new snippet was added
            Listeners.ForEach(listener => listener.SnippetAdded(sender, this, snippet));

            return true;
        }

        public void RemoveSnippet(object sender, AbstractSnippet snippet)
        {
            if (Snippets.Remove(snippet))
            {
                AppSettings.PutAndSave(GetKey(snippet), Snippets);

                // Notify listeners that a snippet was removed
                Listeners.ForEach(listener => listener.SnippetRemoved(sender, this, snippet));
            }
        }

        private string GetKey(AbstractSnippet snippet)
        {
            return snippet switch
            {
                OcrTextSnippet _ => OcrSnippetsKey,
                TextSnippet _ => TextSnippetsKey,
                FileSnippet _ => FileSnippetsKey,
                _ => ""
            };
        }

        public void Unload()
        {
            Snippets.Clear();
        }

        public async Task<bool> Activate(Query query)
        {
            return Meta.Enabled;
        }

        public async IAsyncEnumerable<IQueryResult> Process(Query query)
        {
            foreach (var source in Snippets)
            {
                if (await source.Matches(query))
                {
                    yield return new SnippetQueryResult(source);
                }
            }
        }
    }
}
