using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Notes
{
    public class NotesPlugin : IPlugin
    {
        private static readonly string[] Actions = {
            "notes", // List
            "note" // Create
        };
        private readonly NotesManager _notesManager;

        public NotesPlugin()
        {
            _notesManager = NotesManager.GetInstance();
        }

        public PluginManifest Manifest { get; set; } = new("Notes Plugin", PluginPriority.Highest)
        {
            Description = "Create and view quick notes.\n\nExamples: \"note <new note>\" to create a note, \"notes\" to list existing notes"
        };

        public PluginStorage Storage { get; set; } = new();

        public void Unload() { }

        public async Task<bool> Activate(Query query)
        {
            return Actions.Contains(query.Parts[0]);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
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
