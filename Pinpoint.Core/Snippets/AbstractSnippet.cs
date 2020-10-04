using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pinpoint.Core.Snippets
{
    public abstract class AbstractSnippet
    {
        public AbstractSnippet()
        {
        }

        protected AbstractSnippet(string title, string content)
        {
            Identifier = title;
            FilePath = title;
            RawContent = content;
            Tags = ExtractTags(title);
        }

        public async Task<bool> Applicable(Query query)
        {
            var lowerQuery = query.RawQuery.ToLower();
            return Identifier.ToLower().Contains(lowerQuery) || RawContent.ToLower().Contains(lowerQuery);
        }

        public string[] ExtractTags(string str)
        {
            return new Regex(@"#\w+").Matches(str).Select(match => match.Value).ToArray();
        }

        public string RawContent { get; set; }

        public string Identifier { get; set; }

        public abstract string FilePath { get; set; }

        public string[] Tags { get; set; }

        public abstract void SaveAsJson(bool overwrite = true);
    }
}
