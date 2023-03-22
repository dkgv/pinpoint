using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.OpenAI;

public class OpenAIResult : OpenInNotepadResult
{
    public OpenAIResult(string question, string reply) : base($"Question: {question}{Environment.NewLine}Answer: {reply}")
    {
    }

    public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Brain;
}