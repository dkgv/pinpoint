using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Dictionary
{
    public class DictionaryResult : CopyabableQueryOption
    {
        public DictionaryResult(DefinitionModel definition) : base(definition.PartOfSpeech + ": " + definition.Definition, definition.Definition)
        {
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Book;
    }
}