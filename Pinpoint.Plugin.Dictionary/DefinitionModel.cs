using Newtonsoft.Json;

namespace Pinpoint.Plugin.Dictionary
{
    public class DefinitionModel
    {
        [JsonProperty("definition")]
        public string Definition { get; set; }

        [JsonProperty("example")]
        public string Example { get; set; }

        [JsonProperty("synonyms")]
        public string[] Synonyms { get; set; }

        public string PartOfSpeech { get; set; }
    }
}