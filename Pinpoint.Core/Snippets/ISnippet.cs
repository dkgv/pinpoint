using System.Text.Json.Serialization;
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

        [JsonIgnore]
        public string RawContent { get; set; }

        public string Identifier { get; set; }

        public string FilePath { get; set; }
    }
}
