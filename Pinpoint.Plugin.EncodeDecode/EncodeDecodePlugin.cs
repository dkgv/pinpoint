using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Pinpoint.Core;

namespace Pinpoint.Plugin.EncodeDecode
{
    public class EncodeDecodePlugin: IPlugin
    {
        private readonly string[] _prefixes = new[] {"bin:", "hex:", "b64:"};
        public PluginMeta Meta { get; set; } = new PluginMeta("Encode/Decode Plugin", PluginPriority.NextHighest);
        public void Load()
        {
        }

        public void Unload()
        {
        }

        public Task<bool> Activate(Query query)
        {
            var match = query.RawQuery.Length > 4 && _prefixes.Any(prefix => query.RawQuery.StartsWith(prefix));
            return Task.FromResult(match);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var prefix = _prefixes.FirstOrDefault(pre => query.RawQuery.StartsWith(pre));

            var handler = CreateHandler(prefix);

            if (handler != null)
            {
                var str = query.RawQuery.Substring(4);
                yield return new EncodeDecodeResult(handler.Encode(str));
                yield return new EncodeDecodeResult(handler.Decode(str));
            };
            
        }

        private IEncodeDecodeHandler CreateHandler(string prefix)
        {
            switch (prefix)
            {
                case "hex:":
                    return new HexHandler();
                case "bin:":
                    return new BinaryHandler();
                case "b64:":
                    return new Base64Handler();
                default:
                    return new NullHandler();
            }
        }
    }
}
