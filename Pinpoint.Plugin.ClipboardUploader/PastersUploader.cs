using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Pinpoint.Core;

namespace Pinpoint.Plugin.ClipboardUploader
{
    public class PastersUploader : AbstractUploader
    {
        public override string Name => "https://paste.rs/";

        public override async Task<string> Upload(string content)
        {
            var httpContent = new StringContent(content);
            var response = await HttpHelper.SendPost(Name, httpContent, async message =>
            {
                if (message.StatusCode != HttpStatusCode.Created)
                {
                    return string.Empty;
                }
                return await message.Content.ReadAsStringAsync();
            });
            return await response;
        }
    }
}