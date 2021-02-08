using System;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Pinpoint.Plugin.EncodeDecode
{
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
}