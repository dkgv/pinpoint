using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Notes
{
    public class NotesPlugin : AbstractPlugin
    {
        private static readonly string[] Actions = {
            "notes", // List
            "note" // Create
        };

        private readonly NotesManager _notesManager;

        public NotesPlugin()
        {
            _notesManager = NotesManager.GetInstance(this);
        }

        public override PluginManifest Manifest { get; } = new("Notes Plugin", PluginPriority.High)
        {
            Description = "Create and view quick notes.\n\nExamples: \"note <new note>\" to create a note, \"notes\" to list existing notes"
        };

        public override async Task<bool> ShouldActivate(Query query)
        {
            return Actions.Contains(query.Parts[0]);
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            switch (query.Parts[0])
            {
                case "note":
                    if (query.Parts.Length == 1)
                    {
                        break;
                    }

                    var noteContent = string.Join(' ', query.Parts[1..]);
                    yield return new AddNoteResult(noteContent);
                    break;

                case "notes":
                    var notes = await _notesManager.GetNotes();
                    foreach (var note in notes.OrderByDescending(n => n.CreatedAt))
                    {
                        yield return new NoteResult(note);
                    }
                    break;
            }
        }
    }
}
