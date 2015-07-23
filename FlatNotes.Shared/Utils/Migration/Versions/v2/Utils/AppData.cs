using FlatNotes.Utils.Migration.Versions.v2.Events;
using FlatNotes.Utils.Migration.Versions.v2.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace FlatNotes.Utils.Migration.Versions.v2.Utils
{
    public static class AppData
    {
        public static event EventHandler NotesChanged;
        public static event EventHandler ArchivedNotesChanged;
        public static event EventHandler<NoteEventArgs> NoteCreated;
        public static event EventHandler<NoteEventArgs> NoteChanged;
        public static event EventHandler<NoteEventArgs> NoteArchived;
        public static event EventHandler<NoteEventArgs> NoteRestored;
        public static event EventHandler<NoteIdEventArgs> NoteRemoved;

        public static bool HasUnsavedChangesOnNotes;
        public static bool HasUnsavedChangesOnArchivedNotes;

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
        private static bool isNotesLoaded = false;

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
        private static bool isArchivedNotesLoaded = false;

        //public AppData()
        //{
        //    NotesChanged += (s, e) => Debug.WriteLine("Notes changed");
        //    Notes.CollectionChanged += (s, e) => NotesChanged(s, e);
        //}

        public static async Task Init()
        {
            //versioning -- migrate app data structure when necessary
            await Migration.Migrate(AppSettings.Instance.Version);
        }

        public static async Task LoadNotesIfNecessary()
        {
            if (isNotesLoaded) return;

            AppData.Notes = await AppSettings.Instance.LoadNotes();
            isNotesLoaded = true;
        }

        public static async Task LoadArchivedNotesIfNecessary()
        {
            if (isArchivedNotesLoaded) return;

            AppData.ArchivedNotes = await AppSettings.Instance.LoadArchivedNotes();
            isArchivedNotesLoaded = true;
        }

        public static async Task<bool> SaveNotes()
        {
            Debug.WriteLine("Save notes");
            var success = await AppSettings.Instance.SaveNotes(Notes);

            if (success) HasUnsavedChangesOnNotes = false;
            return success;
        }

        public static async Task<bool> SaveArchivedNotes()
        {
            Debug.WriteLine("Save archived notes");
            var success = await AppSettings.Instance.SaveArchivedNotes(ArchivedNotes);

            if (success) HasUnsavedChangesOnArchivedNotes = false;
            return success;
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
            if (note == null || note.IsEmpty()) return false;
            await LoadNotesIfNecessary();

            Debug.WriteLine("Create note: " + note.Title);
            App.TelemetryClient.TrackEvent("NoteCreated");

            Notes.Insert(0, note);

            bool success = await SaveNotes();
            if (!success) return false;

            note.Changed = false;
            note.AlreadyExists = true;

            var handler = NoteCreated;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        private static async Task<bool> UpdateNote(Note note)
        {
            if (note == null) return false;
            bool isNoteArchived = ArchivedNotes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;

            Debug.WriteLine("Update note: " + note.Title);
            //App.TelemetryClient.TrackEvent("NoteUpdated");

            bool success = isNoteArchived ? await SaveArchivedNotes() : await SaveNotes();
            if (!success) return false;

            note.Changed = false;

            var handler = NoteChanged;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        public static async Task<bool> ArchiveNote(Note note)
        {
            if (note == null) return false;

            bool noteExists = Notes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;
            if (!noteExists) return false;

            Debug.WriteLine("Archive note: " + note.Title);
            //App.TelemetryClient.TrackEvent("NoteArchived");

            await LoadArchivedNotesIfNecessary();
            bool noteAlreadyArchived = ArchivedNotes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;

            if (!noteAlreadyArchived)
            {
                note.ArchivedAt = DateTime.UtcNow;
                ArchivedNotes.Insert(0, note);

                bool success = await SaveArchivedNotes();
                if (!success) return false;
            }

            note.IsArchived = true;
            await RemoveNote(note, true);

            var handler = NoteArchived;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        public static async Task<bool> RestoreNote(Note note)
        {
            if (note == null) return false;
            await LoadArchivedNotesIfNecessary();

            Debug.WriteLine("Restore note: " + note.Title);
            //App.TelemetryClient.TrackEvent("ArchivedNoteRestored");

            bool noteAlreadyArchived = ArchivedNotes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;
            if (!noteAlreadyArchived) return false;

            bool noteExists = Notes.Where<Note>(x => x.ID == note.ID).FirstOrDefault<Note>() != null;

            if (!noteExists)
            {
                Notes.Insert(0, note);

                bool success = await SaveNotes();
                if (!success) return false;
            }

            note.AlreadyExists = true;
            note.IsArchived = false;
            await RemoveArchivedNote(note, true);

            var handler = NoteRestored;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        public static async Task<bool> RemoveNote(Note note, bool isArchiving = false)
        {
            if (note == null) return false;
            string id = note.ID;

            Debug.WriteLine("Remove note: " + note.Title);
            //App.TelemetryClient.TrackEvent("NoteRemoved");

            //remove note images from disk
            if (!isArchiving)
                await RemoveNoteImages(note.Images);

            bool noteExists = Notes.Where<Note>(x => x.ID == id).FirstOrDefault<Note>() != null;
            if (!noteExists) return true;

            Notes.Remove(note);
            bool success = await SaveNotes();

            if (!success) return false;

            if (!isArchiving)
            {
                var handler = NoteRemoved;
                if (handler != null) handler(null, new NoteIdEventArgs(id));
            }

            return true;
        }

        public static async Task<bool> RemoveArchivedNote(Note note, bool isRestoring = false)
        {
            if (note == null) return false;
            string id = note.ID;

            Debug.WriteLine("Remove archived note: " + note.Title);
            //App.TelemetryClient.TrackEvent("ArchivedNoteRemoved");

            //remove note images from disk
            if (!isRestoring)
                await RemoveNoteImages(note.Images);

            bool isArchived = ArchivedNotes.Where<Note>(x => x.ID == id).FirstOrDefault<Note>() != null;
            if (!isArchived) return true;

            ArchivedNotes.Remove(note);
            bool success = await SaveArchivedNotes();

            if (!success) return false;

            if(!isRestoring)
            {
                var handler = NoteRemoved;
                if (handler != null) handler(null, new NoteIdEventArgs(id));
            }

            return true;
        }

        public static async Task<bool> RemoveNoteImages(IList<NoteImage> noteImages)
        {
            if (noteImages == null || noteImages.Count <= 0) return true;
            bool success = true;

            foreach (var noteImage in noteImages)
                success &= await RemoveNoteImage(noteImage);

            return success;
        }

        public static async Task<bool> RemoveNoteImage(NoteImage noteImage)
        {
            if (noteImage == null) return true;
            bool success = true;

            try
            {
                Debug.WriteLine("Delete {0}", noteImage.URL);
                var file = await StorageFile.GetFileFromPathAsync(noteImage.URL);
                await file.DeleteAsync();
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }
    }
}