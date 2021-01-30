namespace Pinpoint.Plugin.EncodeDecode
{
    public class NullHandler: IEncodeDecodeHandler
    {
        public string Encode(string stringToEncode)
        {
            return "Could not recognize format";
        }

        public string Decode(string stringToDecode)
        {
            return "Could not recognize format";
        }
    }
}