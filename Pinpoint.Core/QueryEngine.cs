using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pinpoint.Core.Snippets;

namespace Pinpoint.Core
{
    public class QueryEngine
    {
        public List<ISnippetListener> Listeners { get; } = new List<ISnippetListener>();
        
        public List<ISnippet> Snippets { get; } = new List<ISnippet>();

        public void Initialize()
        {
            void LoadSnippets<T>(string key) where T : ISnippet
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
            LoadSnippets<ManualSnippet>(AppConstants.ManualSnippetsKey);
        }

        public bool AddSnippet(object sender, ISnippet snippet)
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
            Listeners.ForEach(listener => listener.SnippetAdded(sender, snippet));

            return true;
        }

        public void RemoveSnippet(object sender, ISnippet snippet)
        {
            if (Snippets.Remove(snippet))
            {
                AppSettings.PutAndSave(GetKey(snippet), Snippets);

                // Notify listeners that a snippet was removed
                Listeners.ForEach(listener => listener.SnippetRemoved(sender, snippet));
            }
        }

        private string GetKey(ISnippet snippet)
        {
            return snippet is ManualSnippet ? AppConstants.ManualSnippetsKey : AppConstants.FileSnippetsKey;
        }

        public async IAsyncEnumerable<ISnippet> Process(Query query)
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
