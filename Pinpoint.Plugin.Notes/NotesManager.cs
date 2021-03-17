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
        private readonly string _notesFilePath = AppConstants.MainDirectory + "/notes/notes.json";
        private List<Note> _notesCached;
        private static NotesManager _instance;

        public static NotesManager GetInstance()
        {
            return _instance ??= new NotesManager();
        }

        private NotesManager()
        {
            if (!File.Exists(_notesFilePath))
            {
                if (!Directory.Exists(AppConstants.MainDirectory + "/notes"))
                {
                    Directory.CreateDirectory(AppConstants.MainDirectory + "/notes");
                }

                var defaultValue = new NotesFile()
                {
                    Notes = new List<Note>
                    {
                        new Note
                        {
                            Content = "Det her er en note",
                            CreatedAt = DateTime.Now,
                            Id = Guid.NewGuid()
                        },
                        new Note
                        {
                            Content = "Det her er også en note",
                            CreatedAt = DateTime.Now,
                            Id = Guid.NewGuid()
                        }
                    }
                };

                var defaultValueJson = JsonConvert.SerializeObject(defaultValue);
                File.WriteAllText(_notesFilePath, defaultValueJson);
            }

            var json = File.ReadAllText(_notesFilePath);
            var notesFile = JsonConvert.DeserializeObject<NotesFile>(json);

            _notesCached = notesFile.Notes;
        }

        public async Task<List<Note>> GetNotes()
        {
            if (_notesCached != null && _notesCached.Count != 0)
            {
                return _notesCached;
            }

            var json = await File.ReadAllTextAsync(_notesFilePath);
            var notesFile = JsonConvert.DeserializeObject<NotesFile>(json);

            return _notesCached = notesFile.Notes;
        }

        public async Task AddNote(Note note)
        {
            _notesCached.Add(note);

            var json = JsonConvert.SerializeObject(new NotesFile
            {
                Notes = _notesCached
            });

            await File.WriteAllTextAsync(_notesFilePath, json);
        }

        public async Task RemoveNote(Guid noteId)
        {
            var filteredNotes = _notesCached.Where(note => note.Id != noteId).ToList();

            _notesCached = filteredNotes;

            await SaveNotes(filteredNotes);
        }

        private async Task SaveNotes(List<Note> notes)
        {
            var json = JsonConvert.SerializeObject(new NotesFile
            {
                Notes = notes
            });

            await File.WriteAllTextAsync(_notesFilePath, json);
        }
    }
}