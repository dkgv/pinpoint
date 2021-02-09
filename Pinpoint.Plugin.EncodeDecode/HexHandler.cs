using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Pinpoint.Plugin.EncodeDecode
{
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
}