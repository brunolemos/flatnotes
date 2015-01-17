using Keep.Events;
using Keep.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Keep.Utils
{
    public static class AppData
    {
        public static event EventHandler NotesChanged;
        public static event EventHandler ArchivedNotesChanged;
        public static event EventHandler<NoteEventArgs> NoteCreated;
        public static event EventHandler<NoteEventArgs> NoteChanged;
        public static event EventHandler<NoteEventArgs> NoteArchived;
        public static event EventHandler<NoteEventArgs> NoteRestored;
        public static event EventHandler<NoteEventArgs> NoteRemoved;

        public static Stopwatch Watch = new Stopwatch();

        public static Notes Notes
        {
            get { return notes; }

            set
            {
                notes.Clear();
                if (value != null) foreach (var item in value) notes.Add(item);

                var handler = NotesChanged;
                if (handler != null) handler(null, EventArgs.Empty);
            }
        }
        private static Notes notes = new Notes();

        public static Notes ArchivedNotes
        {
            get { return archivedNotes; }

            set
            {
                archivedNotes.Clear();
                if (value != null) foreach (var item in value) archivedNotes.Add(item);

                var handler = ArchivedNotesChanged;
                if (handler != null) handler(null, EventArgs.Empty);
            }
        }
        private static Notes archivedNotes = new Notes();

        public static Note TempNote { get { return tempNote; } set { tempNote = value; } }
        private static Note tempNote = null;

        //public AppData()
        //{
        //    NotesChanged += (s, e) => Debug.WriteLine("Notes changed");
        //    Notes.CollectionChanged += (s, e) => NotesChanged(s, e);
        //}

        public static async Task<bool> SaveNotes()
        {
            Debug.WriteLine("Save notes");
            return await AppSettings.Instance.SaveNotes(Notes);
        }

        public static async Task<bool> SaveArchivedNotes()
        {
            Debug.WriteLine("Save archived notes");
            return await AppSettings.Instance.SaveArchivedNotes(ArchivedNotes);
        }

        public static async Task<bool> CreateOrUpdateNote(Note note)
        {
            bool noteAlreadyExists = Notes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;
            bool isNoteArchived = ArchivedNotes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;

            if (note.IsEmpty())
                return noteAlreadyExists ? await RemoveNote(note) : (isNoteArchived ? await RemoveArchivedNote(note) : false);
            else
                return noteAlreadyExists || isNoteArchived ? await UpdateNote(note) : await CreateNote(note);
        }

        private static async Task<bool> CreateNote(Note note)
        {
            Debug.WriteLine("Create note: " + note.Title);

            Notes.Insert(0, note);

            bool success = await SaveNotes();
            if (!success) return false;

            var handler = NoteCreated;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        private static async Task<bool> UpdateNote(Note note)
        {
            Debug.WriteLine("Update note: " + note.Title);

            bool isNoteArchived = ArchivedNotes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;

            bool success = isNoteArchived ? await SaveArchivedNotes() : await SaveNotes();
            if (!success) return false;

            var handler = NoteChanged;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        public static async Task<bool> ArchiveNote(Note note)
        {
            Debug.WriteLine("Archive note: " + note.Title);

            bool noteExists = Notes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;
            if (!noteExists) return false;

            bool noteAlreadyArchived = ArchivedNotes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;
            if (noteAlreadyArchived) return true;

            ArchivedNotes.Insert(0, note);

            bool success = await SaveArchivedNotes();
            if (!success) return false;

            await RemoveNote(note);

            var handler = NoteArchived;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        public static async Task<bool> RestoreNote(Note note)
        {
            Debug.WriteLine("Restore note: " + note.Title);

            bool noteAlreadyArchived = ArchivedNotes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;
            if (!noteAlreadyArchived) return false;

            bool noteExists = Notes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;
            if (noteExists) return true;

            Notes.Insert(0, note);

            bool success = await SaveNotes();
            if (!success) return false;

            await RemoveArchivedNote(note);

            var handler = NoteRestored;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        public static async Task<bool> RemoveNote(Note note)
        {
            Debug.WriteLine("Remove note: " + note.Title);

            bool noteExists = Notes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;
            if (!noteExists) return true;


            Notes.Remove(note);
            bool success = await SaveNotes();

            if (!success) return false;

            var handler = NoteRemoved;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        public static async Task<bool> RemoveArchivedNote(Note note)
        {
            Debug.WriteLine("Remove archived note: " + note.Title);

            bool isArchived = ArchivedNotes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;
            if (!isArchived) return true;

            ArchivedNotes.Remove(note);
            bool success = await SaveArchivedNotes();

            if (!success) return false;

            var handler = NoteRemoved;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }
    }
}