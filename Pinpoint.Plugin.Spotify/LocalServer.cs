using System.Net;
using System.Text;

namespace PinPoint.Plugin.Spotify
{
    public class LocalServer
    {
        private readonly byte[] _buffer =
            Encoding.UTF8.GetBytes(@"<html><body><h2>Spotify authentication was succesful - you can safely close this window now.</h2></body></html>");
        private readonly HttpListener _listener = new()
        {
            Prefixes = { "http://localhost:5001/" }
        };

        public void Launch()
        {
            _listener.Start();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        public HttpListenerContext RetrieveContext()
        {
            var ctx = _listener.GetContext();

            var response = ctx.Response;

            response.ContentLength64 = _buffer.Length;
            var outputStream = response.OutputStream;
            
            outputStream.Write(_buffer, 0, _buffer.Length);
            outputStream.Close();

            return ctx;
        }
    }
}