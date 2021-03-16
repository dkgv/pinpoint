using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Notes
{
    public class NoteResult: AbstractFontAwesomeQueryResult
    {
        public NoteResult(Note note): base(note.Content)
        {
            Subtitle = $"Created at {note.CreatedAt.ToShortDateString()}";

            Options.Add(new DeleteNoteOptionResult(note));
        }

        public override void OnSelect()
        {
            
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_List;
    }
}