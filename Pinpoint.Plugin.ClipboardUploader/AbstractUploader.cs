using System.Net.Http;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.ClipboardUploader
{
    public abstract class AbstractUploader
    {
        public abstract string Name { get; }

        public abstract Task<string> Upload(string content);
    }
}