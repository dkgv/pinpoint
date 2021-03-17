using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Notes
{
    public class NotesPlugin : IPlugin
    {
        private static readonly string[] Actions = {
            "notes-add",
            "n+"
        };
        private readonly NotesManager _notesManager;

        public NotesPlugin()
        {
            _notesManager = NotesManager.GetInstance();
        }

        public PluginMeta Meta { get; set; } = new PluginMeta("Notes Plugin", PluginPriority.Highest);
        
        public bool TryLoad() => true;

        public void Unload() { }

        public async Task<bool> Activate(Query query)
        {
            return Actions.Contains(query.Parts[0]);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var notes = await _notesManager.GetNotes();
            var prefix = query.Parts[0];

            switch (prefix)
            {
                case "n+":
                case "notes-add":
                    var noteContent = query.RawQuery.Replace(prefix, "").TrimStart();

                    if (!string.IsNullOrWhiteSpace(noteContent))
                    {
                        yield return new AddNoteResult(noteContent);
                    }

                    break;
            }

            foreach (var note in notes.OrderByDescending(n => n.CreatedAt))
            {
                yield return new NoteResult(note);
            }
        }
    }
}
