using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pinpoint.Core
{
    public static class HttpRequestHandler
    {
        private static readonly HttpClient HttpClient = new();

        public static async Task<T> SendGet<T>(string url, Func<string, T> parse)
        {
            try
            {
                var response = await HttpClient.GetStringAsync(url);
                return string.IsNullOrEmpty(response) || response.Contains("error") ? default : parse(response);
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