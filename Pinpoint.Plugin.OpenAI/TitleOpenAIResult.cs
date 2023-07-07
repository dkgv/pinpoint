using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.OpenAI;

public class TitleOpenAIResult : AbstractFontAwesomeQueryResult
{
    public TitleOpenAIResult() : base("Waiting for OpenAI", "Typing will interrupt current request.")
    {
    }

    public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Question;
    
    public override void OnSelect()
    {
    }
}