using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pinpoint.Core;

namespace Pinpoint.Plugin.Notes
{
    public class NotesManager
    {
        private List<Note> _notesCached = new();
        private readonly NotesPlugin _notes;
        private static NotesManager _instance;

        public static NotesManager GetInstance(NotesPlugin notes = null)
        {
            return _instance ??= new NotesManager(notes);
        }

        private NotesManager(NotesPlugin notes)
        {
            _notes = notes;
        }

        public async Task<List<Note>> GetNotes()
        {
            if (_notesCached != null && _notesCached.Count != 0)
            {
                return _notesCached;
            }

            var notes = _notes.Storage.User["notes"];
            if (notes != null)
            {
                return _notesCached = JsonConvert.DeserializeObject<List<Note>>(notes.ToString());
            }

            return _notesCached = new List<Note>();
        }

        public async Task AddNote(Note note)
        {
            _notesCached.Add(note);
            await SaveNotes();
        }

        public async Task RemoveNote(Guid noteId)
        {
            var filteredNotes = _notesCached.Where(note => note.Id != noteId).ToList();

            _notesCached = filteredNotes;
            await SaveNotes();
        }

        private async Task SaveNotes()
        {
            _notes.Storage.User["notes"] = _notesCached;
            _notes.Save();
        }
    }
}