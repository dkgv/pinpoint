using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
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
                case "bin:":
                    return new BinaryHandler();
                case "b64:":
                    return new Base64Handler();
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

    public class Base64Handler: IEncodeDecodeHandler
    {
        private readonly Regex _decodeRegex = new Regex(@"[^-A-Za-z0-9+/=]|=[^=]|={3,}$");

        public string Encode(string stringToEncode)
        {
            var bytes = Encoding.UTF8.GetBytes(stringToEncode);
            return Convert.ToBase64String(bytes);
        }

        public string Decode(string stringToDecode)
        {
            if (!_decodeRegex.IsMatch(stringToDecode)) return "Could not decode as hex";

            var bytes = Convert.FromBase64String(stringToDecode);

            return Encoding.UTF8.GetString(bytes);
        }
    }

    public class BinaryHandler: IEncodeDecodeHandler
    {
        private readonly Regex _encodeRegex = new Regex(@"^[0-9]+$");
        private readonly Regex _decodeRegex = new Regex(@"^[0-1]+$");

        //Blatantly stolen from this answer: https://stackoverflow.com/a/15447131/7230001
        public string Encode(string number)
        {
            if (!_encodeRegex.IsMatch(number)) return "Could not encode as binary";

            var bigInteger = BigInteger.Parse(number);

            var bytes = bigInteger.ToByteArray();
            var idx = bytes.Length - 1;

            // Create a StringBuilder having appropriate capacity.
            var base2 = new StringBuilder(bytes.Length * 8);

            // Convert first byte to binary.
            var binary = Convert.ToString(bytes[idx], 2);

            // Ensure leading zero exists if value is positive.
            if (binary[0] != '0' && bigInteger.Sign == 1)
            {
                base2.Append('0');
            }

            // Append binary string to StringBuilder.
            base2.Append(binary);

            // Convert remaining bytes adding leading zeros.
            for (idx--; idx >= 0; idx--)
            {
                base2.Append(Convert.ToString(bytes[idx], 2).PadLeft(8, '0'));
            }

            return base2.ToString();
        }

        public string Decode(string binaryString)
        {
            if (!_decodeRegex.IsMatch(binaryString)) return "Could not decode as binary";

            BigInteger one = 1;
            BigInteger result = 0;

            foreach (var c in binaryString)
            {
                result <<= 1;
                if (c == '1')
                {
                    result |= one;
                }
            }

            return result.ToString();
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
