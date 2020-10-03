using System.Threading.Tasks;

namespace Pinpoint.Core.Snippets
{
    public interface ISnippet
    {
        public async Task<bool> Applicable(Query query)
        {
            var lowerQuery = query.RawQuery.ToLower();
            return Identifier.ToLower().Contains(lowerQuery) || RawContent.ToLower().Contains(lowerQuery);
        }

        public string RawContent { get; set; }

        public string Identifier { get; set; }

        public string FilePath { get; set; }

        void SaveAsJSON(bool overwrite = true);

        void SaveAsMarkdown();
    }
}
