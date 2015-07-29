using FlatNotes.Events;
using FlatNotes.Models;
using SQLite.Net;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace FlatNotes.Utils
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
        public static event EventHandler<NoteColorEventArgs> NoteColorChanged;

        public static SQLiteConnection DB { get { if (db == null) InitDB(); return db; } }
        private static SQLiteConnection db;

        public static Notes Notes { get { if (notes == null) LoadNotesIfNecessary(); return notes; } private set { notes = value; } }
        private static Notes notes;

        public static Notes ArchivedNotes { get { if (archivedNotes == null) LoadArchivedNotesIfNecessary(); return archivedNotes; } private set { archivedNotes = value; } }
        private static Notes archivedNotes;

        public static async Task Init()
        {
            //versioning -- migrate app data structure when necessary
            await Migration.Migration.Migrate(AppSettings.Instance.Version);

            if(db == null)
                InitDB();
        }

        private static void InitDB()
        {
            ConnectDB();
            CreateTables();
        }

        private static void ConnectDB()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.RoamingFolder.Path, "app.db");
            db = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
        }

        private static void CreateTables()
        {
            DB.Execute("PRAGMA foreign_keys = ON;");

            //create just the primary keys / foreign keys due to sqlite limitation
            DB.Execute(@"CREATE TABLE IF NOT EXISTS Note (ID varchar PRIMARY KEY NOT NULL)");
            DB.Execute(@"CREATE TABLE IF NOT EXISTS ChecklistItem (ID varchar PRIMARY KEY NOT NULL, NoteId varchar REFERENCES Note (ID) ON DELETE CASCADE)");
            DB.Execute(@"CREATE TABLE IF NOT EXISTS NoteImage (ID varchar PRIMARY KEY NOT NULL, NoteId varchar REFERENCES Note (ID) ON DELETE CASCADE)");

            //create indexes (another sqlite limitation)
            DB.CreateIndex("ChecklistItem", "NoteId");
            DB.CreateIndex("NoteImage", "NoteId");

            //create rest of the table (if not exists)
            DB.CreateTable<ChecklistItem>();
            DB.CreateTable<NoteImage>();
            DB.CreateTable<Note>();
        }

        public static void LoadNotesIfNecessary()
        {
            if (notes != null && notes.Count > 0) return;

            notes = DB.GetAllWithChildren<Note>(x => x.IsArchived != true).OrderByDescending(x => x.Order).ThenByDescending(x => x.CreatedAt).ToList();
            if (notes == null) notes = new Notes();
        }

        public static void LoadArchivedNotesIfNecessary()
        {
            if (archivedNotes != null && archivedNotes.Count > 0) return;

            archivedNotes = DB.GetAllWithChildren<Note>(x => x.IsArchived == true).OrderByDescending(x => x.ArchivedAt).ToList();
            if (archivedNotes == null) archivedNotes = new Notes();
        }
        
        public static Note TryGetNoteById(string id)
        {
            if(notes != null && notes.Count > 0)
            {
                var note = notes.FirstOrDefault(x => x.ID == id);
                if (note != null) return note;
            }

            return DB.GetWithChildren<Note>(id);
        }

        public static async Task<bool> CreateOrUpdateNote(Note note)
        {
            if (note == null) return false;

            bool alreadyExists = DB.Find<Note>(note.ID) != null;
            
            if (note.IsEmpty())
                return await RemoveNote(note);
            else
                return alreadyExists ? UpdateNote(note) : CreateNote(note);
        }

        private static bool CreateNote(Note note)
        {
            if (note == null || note.IsEmpty()) return false;

            Debug.WriteLine("Create note: " + note.Title);
            //App.TelemetryClient.TrackEvent("NoteCreated");

            //associate with note
            foreach (var item in note.Checklist) item.NoteId = note.ID;
            foreach (var item in note.Images) item.NoteId = note.ID;

            if (note.CreatedAt == null) note.TouchCreatedAt();
            if (note.UpdatedAt == null) note.Touch();

            note.Changed = false;
            note.Order = Notes.Count;
            AppData.Notes.Insert(0, note);

            DB.InsertWithChildren(note);

            var handler = NoteCreated;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        private static bool UpdateNote(Note note)
        {
            if (note == null) return false;

            Debug.WriteLine("Update note: " + note.Title);
            //App.TelemetryClient.TrackEvent("NoteUpdated");

            //associate with note
            foreach (var item in note.Checklist) item.NoteId = note.ID;
            foreach (var item in note.Images) item.NoteId = note.ID;

            //DB.UpdateWithChildren(note);
            bool success = DB.Update(note) == 1;
            if (!success) return false;

            //update checklist items and note images
            if (note.Checklist.Count > 0) DB.InsertOrReplaceAll(note.Checklist);
            if (note.Images.Count > 0) DB.InsertOrReplaceAll(note.Images);

            note.Changed = false;

            var handler = NoteChanged;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        public static bool ArchiveNote(Note note)
        {
            if (note == null) return false;

            Debug.WriteLine("Archive note: " + note.Title);
            //App.TelemetryClient.TrackEvent("NoteArchived");

            LoadArchivedNotesIfNecessary();
            note.IsArchived = true;
            note.TouchArchivedAt();

            bool success = DB.Update(note) == 1;
            if (!success) return false;

            AppData.Notes.Remove(note);
            AppData.ArchivedNotes.Insert(0, note);
            Debug.WriteLine("AppData.ArchivedNotes.Insert(0, note);");

            var handler = NoteArchived;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        public static bool RestoreNote(Note note)
        {
            if (note == null) return false;

            Debug.WriteLine("Restore note: " + note.Title);
            //App.TelemetryClient.TrackEvent("ArchivedNoteRestored");

            note.IsArchived = false;
            note.Order = Notes.Count;

            bool success = DB.Update(note) == 1;
            if (!success) return false;

            AppData.Notes.Insert(0, note);
            AppData.ArchivedNotes.Remove(note);

            var handler = NoteRestored;
            if (handler != null) handler(null, new NoteEventArgs(note));

            return true;
        }

        public static bool ChangeNoteColor(Note note, NoteColor newColor)
        {
            if (note == null) return false;

            Debug.WriteLine("Change note color: " + newColor.Key);
            //App.TelemetryClient.TrackEvent("NoteColorChanged");

            note.Color = newColor;

            bool success = DB.Update(note) == 1;
            if (!success) return false;

            var handler = NoteColorChanged;
            if (handler != null) handler(null, new NoteColorEventArgs(note, newColor));

            return true;
        }

        public static async Task<bool> RemoveNote(Note note)
        {
            if (note == null) return false;

            Debug.WriteLine("Remove note: " + note.Title);
            //App.TelemetryClient.TrackEvent("NoteRemoved");

            //remove note images from disk
            await RemoveNoteImages(note.Images);

            DB.Delete(note);

            AppData.Notes.Remove(note);
            AppData.ArchivedNotes.Remove(note);

            var handler = NoteRemoved;
            if (handler != null) handler(null, new NoteIdEventArgs(note.ID));

            return true;
        }

        public static async Task<bool> RemoveNoteImages(IList<NoteImage> noteImages, bool deleteFromDB = true)
        {
            if (noteImages == null || noteImages.Count <= 0) return true;
            bool success = true;

            foreach (var noteImage in noteImages)
                success &= await RemoveNoteImage(noteImage, deleteFromDB);

            return success;
        }

        public static async Task<bool> RemoveNoteImage(NoteImage noteImage, bool deleteFromDB = true)
        {
            if (noteImage == null) return true;
            bool success = true;

            try
            {
                Debug.WriteLine("Delete {0}", noteImage.URL);
                var file = await StorageFile.GetFileFromPathAsync(noteImage.URL);
                await file.DeleteAsync();

                if (deleteFromDB)
                    DB.Delete(noteImage);
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }
    }
}