using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Core;

namespace Pinpoint.Plugin.EncodeDecode
{
    public class EncodeDecodePlugin: IPlugin
    {
        private static readonly QueryMatch[] Prefixes =
            {QueryMatch.CreateHexQueryMatch(), QueryMatch.CreateBase64QueryMatch()};

        public PluginMeta Meta { get; set; } = new PluginMeta("Encode/Decode Plugin", PluginPriority.NextHighest);
        public void Load()
        {
        }

        public void Unload()
        {
        }

        public Task<bool> Activate(Query query)
        { 
            return Task.FromResult(Prefixes.Any(prefix => prefix.HasMatch(query.RawQuery)));
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            foreach (var queryMatch in Prefixes)
            {
                if (!queryMatch.HasMatch(query.RawQuery)) continue;

                var stringToDecode = query.RawQuery.Substring(queryMatch.Identifier.Length);
                var results = EncodeHex(stringToDecode);

                yield return results.Encoded;
                yield return results.Decoded;
            }
        }

        private EncodeDecodeResults EncodeHex(string str)
        {
            var strBytes = Encoding.UTF8.GetBytes(str);

            return new EncodeDecodeResults
            {
                Encoded = new EncodeDecodeResult(BitConverter.ToString(strBytes).Replace("-", "")),
                Decoded = new EncodeDecodeResult(DecodeHex(str))
            };
        }

        private string DecodeHex(string hexString)
        {
            var hex = hexString.Replace("-", "");

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return Encoding.ASCII.GetString(bytes);
            
        }
    }

    public class EncodeDecodeResults
    {
        public EncodeDecodeResult Encoded { get; set; }
        public EncodeDecodeResult Decoded { get; set; }
    }

    public class QueryMatch
    {
        private readonly string _identifier;
        private readonly Regex _regex;

        public string Identifier => _identifier;

        private QueryMatch(string regex, string identifier)
        {
            _identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            _regex = new Regex(regex ?? throw new ArgumentNullException(nameof(regex)));
        }

        public bool HasMatch(string query) => query != null && query.Length > _identifier.Length;

        public static QueryMatch CreateHexQueryMatch() => new QueryMatch("/[0-9a-f]+/i", "hex:");

        public static QueryMatch CreateBase64QueryMatch() => new QueryMatch("regex here", "b64;");
    }

    public class EncodeDecodeResult: AbstractFontAwesomeQueryResult
    {
        public EncodeDecodeResult(string result): base(result)
        {
            
        }
        public override void OnSelect()
        {
            
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Hashtag;
    }
}
