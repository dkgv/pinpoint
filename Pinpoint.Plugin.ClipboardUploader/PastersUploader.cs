using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.ClipboardUploader
{
    public class PastersUploader : AbstractUploader
    {
        public override string Name => "https://paste.rs/";

        public override async Task<string> Upload(string content)
        {
            var httpContent = new StringContent(content);
            var response = await SendPost(Name, httpContent);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                return string.Empty;
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}