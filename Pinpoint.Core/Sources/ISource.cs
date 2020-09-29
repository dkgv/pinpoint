using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pinpoint.Core.Sources
{
    public interface ISource
    {
        public async Task<bool> Applicable(Query query)
        {
            return Identifier.Contains(query.RawQuery) || RawContent.Contains(query.RawQuery);
        }

        [JsonIgnore]
        public string RawContent { get; set; }

        public string Identifier { get; set; }

        public string Location { get; set; }
    }
}
