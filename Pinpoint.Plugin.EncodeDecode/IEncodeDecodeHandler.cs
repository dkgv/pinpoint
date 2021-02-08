namespace Pinpoint.Plugin.EncodeDecode
{
    public interface IEncodeDecodeHandler
    {
        string Encode(string stringToEncode);
        string Decode(string stringToDecode);
    }
}