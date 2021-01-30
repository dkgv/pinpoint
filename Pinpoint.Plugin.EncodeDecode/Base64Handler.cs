using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Pinpoint.Plugin.EncodeDecode
{
    public class Base64Handler: IEncodeDecodeHandler
    {
        private readonly Regex _decodeRegex = new Regex(@"^[A-Za-z0-9\-\+\={0,2}]+$");

        public string Encode(string stringToEncode)
        {
            var bytes = Encoding.UTF8.GetBytes(stringToEncode);
            return Convert.ToBase64String(bytes);
        }

        public string Decode(string stringToDecode)
        {
            var base64WithPadding = ApplyPadding(stringToDecode);
            if (base64WithPadding.Length % 4 != 0 || !_decodeRegex.IsMatch(base64WithPadding)) return "Could not decode as base 64";

            var bytes = Convert.FromBase64String(base64WithPadding);

            return Encoding.UTF8.GetString(bytes);
        }

        private string ApplyPadding(string base64String)
        {
            var stringBuilder = new StringBuilder(base64String);
            var count = 0;

            while (stringBuilder.Length % 4 != 0 && count < 2)
            {
                stringBuilder.Append('=');
                count++;
            }

            var result = stringBuilder.ToString();
            var endIndex = result.Length - (result.Length % 4);
            return result[..endIndex];
        }
    }
}