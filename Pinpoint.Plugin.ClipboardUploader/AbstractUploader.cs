using System.Net.Http;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.ClipboardUploader
{
    public abstract class AbstractUploader
    {
        public abstract string Name { get; }

        public abstract Task<string> Upload(string content);

        protected async Task<HttpResponseMessage> SendPost(string url, HttpContent content)
        {
            try
            {
                using var httpClient = new HttpClient();
                return await httpClient.PostAsync(url, content);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}