using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core.Results;
using Pinpoint.Core;

namespace Pinpoint.Plugin.EncodeDecode
{
    public class EncodeDecodePlugin : IPlugin
    {
        private static readonly string[] Prefixes = { "bin", "hex", "b64" };

        public PluginManifest Manifest { get; set; } = new("Encode/Decode Plugin");

        public PluginStorage Storage { get; set; } = new();

        public void Unload()
        {
        }

        public Task<bool> Activate(Query query)
        {
            var match = query.RawQuery.Length > 4 && Prefixes.Any(prefix => query.RawQuery.StartsWith(prefix));
            return Task.FromResult(match);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var prefix = Prefixes.FirstOrDefault(pre => query.RawQuery.StartsWith(pre));

            var handler = CreateHandler(prefix);
            var queryParts = query.RawQuery.Split(' ');

            if (handler != null && queryParts.Length > 1)
            {
                var queryString = queryParts.Last().Trim();

                yield return new EncodeDecodeResult(handler.Encode(queryString));
                yield return new EncodeDecodeResult(handler.Decode(queryString));
            }
        }

        private IEncodeDecodeHandler CreateHandler(string prefix)
        {
            return prefix switch
            {
                "hex" => new HexHandler(),
                "bin" => new BinaryHandler(),
                "b64" => new Base64Handler(),
                _ => new NullHandler(),
            };
        }
    }
}
