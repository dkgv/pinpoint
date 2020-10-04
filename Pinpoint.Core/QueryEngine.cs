using System.Collections.Generic;
using System.Linq;
using Pinpoint.Core.Snippets;

namespace Pinpoint.Core
{
    public class QueryEngine
    {
        public List<ISnippetListener> Listeners { get; } = new List<ISnippetListener>();
        
        public List<AbstractSnippet> Snippets { get; } = new List<AbstractSnippet>();

        public void Initialize()
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

            LoadSnippets<FileSnippet>(AppConstants.FileSnippetsKey);
            LoadSnippets<TextSnippet>(AppConstants.TextSnippetsKey);
            LoadSnippets<OcrTextSnippet>(AppConstants.OcrSnippetsKey);
        }

        public bool AddSnippet(object sender, AbstractSnippet abstractSnippet)
        {
            // Prevents duplicate sources
            if (Snippets.Contains(abstractSnippet))
            {
                return false;
            }

            Snippets.Add(abstractSnippet);

            // Update settings to include newly added snippet
            var snippetsOfType = Snippets.Where(s => s.GetType() == abstractSnippet.GetType());
            AppSettings.PutAndSave(GetKey(abstractSnippet), snippetsOfType);

            // Notify listeners that a new snippet was added
            Listeners.ForEach(listener => listener.SnippetAdded(sender, abstractSnippet));

            return true;
        }

        public void RemoveSnippet(object sender, AbstractSnippet abstractSnippet)
        {
            if (Snippets.Remove(abstractSnippet))
            {
                AppSettings.PutAndSave(GetKey(abstractSnippet), Snippets);

                // Notify listeners that a snippet was removed
                Listeners.ForEach(listener => listener.SnippetRemoved(sender, abstractSnippet));
            }
        }

        private string GetKey(AbstractSnippet snippet)
        {
            return snippet switch
            {
                OcrTextSnippet _ => AppConstants.OcrSnippetsKey,
                TextSnippet _ => AppConstants.TextSnippetsKey,
                FileSnippet _ => AppConstants.FileSnippetsKey,
                _ => ""
            };
        }

        public async IAsyncEnumerable<AbstractSnippet> Process(Query query)
        {
            foreach (var source in Snippets)
            {
                if (await source.Applicable(query))
                {
                    yield return source;
                }
            }
        }
    }
}
