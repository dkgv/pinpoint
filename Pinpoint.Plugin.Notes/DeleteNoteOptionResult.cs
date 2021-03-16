using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Notes
{
    public class DeleteNoteOptionResult: AbstractFontAwesomeQueryResult
    {
        private readonly Note _note;

        public DeleteNoteOptionResult(Note note): base("Delete note")
        {
            _note = note;
        }
        public override void OnSelect()
        {
            NotesManager.GetInstance().RemoveNote(_note.Id);
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Trash;
    }
}