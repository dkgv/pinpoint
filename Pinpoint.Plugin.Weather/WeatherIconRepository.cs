using System.Collections.Generic;
using System.Drawing;
using System.Net;

namespace Pinpoint.Plugin.Weather
{
    public static class WeatherIconRepository
    {
        private static readonly Dictionary<string, Bitmap> Icons = new Dictionary<string, Bitmap>();

        public static Bitmap Get(string url)
        {
            if (!url.StartsWith("http"))
            {
                url = "https:" + url;
            }

            if (Icons.ContainsKey(url))
            {
                return Icons[url];
            }

            var request = WebRequest.Create(url);
            var response = request.GetResponse();
            var bitmap = new Bitmap(response.GetResponseStream());
            return Icons[url] = bitmap;
        }
    }
}
