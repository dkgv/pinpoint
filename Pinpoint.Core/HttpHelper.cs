using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pinpoint.Core
{
    public static class HttpHelper
    {
        private static readonly HttpClient HttpClient = new();

        public static async Task<T> SendGet<T>(string url, Func<string, T> parse)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Headers = {
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36" }
                    }
                };

                var response = await HttpClient.SendAsync(request);
                return !response.IsSuccessStatusCode ? default : parse(await response.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException)
            {
                return default;
            }
        }

        public static async Task<T> SendPost<T>(string url, HttpContent content, Func<HttpResponseMessage, T> parse)
        {
            try
            {
                var response = await HttpClient.PostAsync(url, content);
                return !response.IsSuccessStatusCode ? default : parse(response);
            }
            catch (HttpRequestException)
            {
                return default;
            }
        }
    }
}