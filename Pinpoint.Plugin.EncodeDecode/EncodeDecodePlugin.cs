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
                default:
                    return new HexHandler();
            }
        }
    }

    public interface IEncodeDecodeHandler
    {
        string Encode(string stringToEncode);
        string Decode(string stringToDecode);
    }

    public class HexHandler: IEncodeDecodeHandler
    {
        private readonly Regex _decodeRegex = new Regex(@"^[0-9a-f\-]+$");
        public string Encode(string str)
        {
            var strBytes = Encoding.ASCII.GetBytes(str);
            return BitConverter.ToString(strBytes).Replace("-", "");
        }

        public string Decode(string hexString)
        {
            if (!_decodeRegex.IsMatch(hexString)) return "Not valid hex.";
            var hex = hexString.Replace("-", "");

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return Encoding.ASCII.GetString(bytes);
        }
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
