using System;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Notes
{
    public class AddNoteResult: AbstractFontAwesomeQueryResult
    {
        private readonly string _content;

        public AddNoteResult(string content): base("Save Note", content)
        {
            _content = content;
        }
        
        public override void OnSelect()
        {
            var note = new Note
            {
                Content = _content,
                CreatedAt = DateTime.Now,
                Id = Guid.NewGuid()
            };

            NotesManager.GetInstance().AddNote(note);
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Solid_Plus;
    }
}