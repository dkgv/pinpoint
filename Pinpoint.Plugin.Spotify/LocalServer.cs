using System.Net;

namespace PinPoint.Plugin.Spotify
{
    public class LocalServer
    {
        private readonly HttpListener _listener = new HttpListener
        {
            Prefixes = { "http://localhost:5001/" }
        };

        public void Launch()
        {
            _listener.Start();
        }

        public HttpListenerContext RetrieveContext()
        {
            var ctx = _listener.GetContext();
            _listener.Stop();
            return ctx;
        }
    }
}