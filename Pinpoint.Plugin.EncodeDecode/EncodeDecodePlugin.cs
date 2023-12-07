using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.EncodeDecode
{
    public class EncodeDecodePlugin : AbstractPlugin
    {
        private static readonly string[] Prefixes = { "bin", "hex", "b64" };

        public override PluginManifest Manifest { get; } = new("Encode/Decode Plugin");

        public override Task<bool> ShouldActivate(Query query)
        {
            var match = query.Raw.Length > 4 && Prefixes.Any(prefix => query.Raw.StartsWith(prefix));
            return Task.FromResult(match);
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var prefix = Prefixes.FirstOrDefault(pre => query.Raw.StartsWith(pre));

            var handler = CreateHandler(prefix);
            var queryParts = query.Raw.Split(' ');

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
