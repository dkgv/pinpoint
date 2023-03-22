using FontAwesome5;

namespace Pinpoint.Core.Results;

public class OpenInNotepadResult : AbstractFontAwesomeQueryResult
{
    private readonly string _content;
    
    public OpenInNotepadResult(string content) : base("Press \"Enter\" to open result in notepad", content)
    {
        _content = content;
    }
    
    public override void OnSelect()
    {
        ProcessHelper.OpenNotepad(_content);
    }

    public override EFontAwesomeIcon FontAwesomeIcon { get; }
}