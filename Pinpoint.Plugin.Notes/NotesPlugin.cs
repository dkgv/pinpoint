using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Notes
{
    public class NotesPlugin : IPlugin
    {
        private readonly NotesManager _notesManager;

        public NotesPlugin()
        {
            _notesManager = NotesManager.GetInstance();
        }

        public PluginMeta Meta { get; set; } = new PluginMeta("Notes plugin", PluginPriority.Highest);
        
        public bool TryLoad() => true;

        public void Unload() { }

        public Task<bool> Activate(Query query)
        {
            return Task.FromResult(query.RawQuery.StartsWith("notes"));
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var notes = await _notesManager.GetNotes();

            if (query.Parts.Length > 0 && query.Parts[0] == "notes+" || query.Parts[0] == "notes-add")
            {
                var noteContent = query.RawQuery.Replace(query.Parts[0], "").TrimStart();

                if (!string.IsNullOrWhiteSpace(noteContent))
                {
                    yield return new AddNoteResult(noteContent);
                }
            }

            foreach (var note in notes.OrderByDescending(n => n.CreatedAt))
            {
                yield return new NoteResult(note);
            }
        }
    }
}
