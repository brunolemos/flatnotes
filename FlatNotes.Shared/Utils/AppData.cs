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
        public static event EventHandler NotesSaved;
        public static event EventHandler NotesChanged;
        public static event EventHandler ArchivedNotesChanged;
        public static event EventHandler<NoteEventArgs> NoteCreated;
        public static event EventHandler<NoteEventArgs> NoteChanged;
        public static event EventHandler<NoteEventArgs> NoteArchived;
        public static event EventHandler<NoteEventArgs> NoteRestored;
        public static event EventHandler<NoteIdEventArgs> NoteRemoved;
        public static event EventHandler<NoteColorEventArgs> NoteColorChanged;

        public static SQLiteConnection LocalDB { get { if (localDB == null) InitLocalDB(); return localDB; } }
        private static SQLiteConnection localDB;

        public static SQLiteConnection RoamingDB { get { if (roamingDB == null) InitRoamingDB(); return roamingDB; } }
        private static SQLiteConnection roamingDB;

        public static Notes Notes { get { if (notes == null) LoadNotesIfNecessary(); return notes; } private set { notes = value; } }
        private static Notes notes;

        public static Notes ArchivedNotes { get { if (archivedNotes == null) LoadArchivedNotesIfNecessary(); return archivedNotes; } private set { archivedNotes = value; } }
        private static Notes archivedNotes;

        public static async Task Init()
        {
            //versioning -- migrate app data structure when necessary
            await Migration.Migration.Migrate(AppSettings.Instance.Version);

            if(localDB == null) InitLocalDB();
            if(roamingDB == null) InitRoamingDB();

            ApplicationData.Current.DataChanged += OnRoamingDataChanged;
        }

        private static void InitLocalDB()
        {
            ConnectLocalDB();
            CreateTablesIfNotCreated(LocalDB);
        }

        private static void InitRoamingDB()
        {
            ConnectRoamingDB();
            CreateTablesIfNotCreated(RoamingDB);
        }

        private static void ConnectLocalDB()
        {
            var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppSettings.DB_FILE_NAME);
            localDB = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
        }

        private static void ConnectRoamingDB() {
            CreateRoamingDBWithLocalDataIfNotExists();

            var path = Path.Combine(ApplicationData.Current.RoamingFolder.Path, AppSettings.DB_FILE_NAME);
            roamingDB = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
        }

        private static void CreateRoamingDBWithLocalDataIfNotExists()
        {
            Task.Run(async () =>
            {
                try
                {
                    var file = await ApplicationData.Current.LocalFolder.GetFileAsync(AppSettings.DB_FILE_NAME);
                    await file.CopyAsync(ApplicationData.Current.RoamingFolder, AppSettings.DB_FILE_NAME, NameCollisionOption.FailIfExists);
                }
                catch (Exception)
                {
                }
            }).Wait();
        }

        private static void CreateTablesIfNotCreated(SQLiteConnection db)
        {
            db.Execute("PRAGMA foreign_keys = ON;");

            //create just the primary keys / foreign keys due to sqlite limitation
            db.Execute(@"CREATE TABLE IF NOT EXISTS Note (ID varchar PRIMARY KEY NOT NULL)");
            db.Execute(@"CREATE TABLE IF NOT EXISTS ChecklistItem (ID varchar PRIMARY KEY NOT NULL, NoteId varchar REFERENCES Note (ID) ON DELETE CASCADE)");
            db.Execute(@"CREATE TABLE IF NOT EXISTS NoteImage (ID varchar PRIMARY KEY NOT NULL, NoteId varchar REFERENCES Note (ID) ON DELETE CASCADE)");

            //create indexes (another sqlite limitation)
            db.CreateIndex("ChecklistItem", "NoteId");
            db.CreateIndex("NoteImage", "NoteId");

            //create rest of the table (if not exists)
            db.CreateTable<ChecklistItem>();
            db.CreateTable<NoteImage>();
            db.CreateTable<Note>();
        }

        private static void OnRoamingDataChanged(ApplicationData sender, object args)
        {
            Debug.WriteLine("OnRoamingDataChanged: {0}, {1}, {2}", sender, args, Newtonsoft.Json.JsonConvert.SerializeObject(args));
            App.TelemetryClient.TrackEvent("OnRoamingDataChanged");

            MergeRoamingWithLocalDB();
        }

        private static async void MergeRoamingWithLocalDB()
        {
            var allLocalNotes = LocalDB.GetAllWithChildren<Note>();
            var allRoamingNotes = RoamingDB.GetAllWithChildren<Note>();

            foreach (var roamingNote in allRoamingNotes)
            {
                var localNote = LocalDB.Find<Note>(roamingNote.ID);

                //create local note
                if (localNote == null)
                {
                    CreateNote(roamingNote, false);
                    Debug.WriteLine("Roaming: Note created");
                }

                //archive local note
                else if (roamingNote.IsArchived && !localNote.IsArchived && roamingNote.ArchivedAt > localNote.UpdatedAt)
                {
                    ArchiveNote(localNote, false);
                    UpdateNote(roamingNote, false);
                    localNote.ReplaceWith(roamingNote);
                    Debug.WriteLine("Roaming: Note archived");
                }

                //delete local note
                else if (roamingNote.DeletedAt != localNote.DeletedAt && roamingNote.DeletedAt > localNote.UpdatedAt)
                {
                    await RemoveNote(localNote, false);
                    UpdateNote(roamingNote, false);
                    localNote.ReplaceWith(roamingNote);
                    Debug.WriteLine("Roaming: Note deleted");
                }

                //update local note
                else if (roamingNote.UpdatedAt > localNote.UpdatedAt)
                {
                    UpdateNote(roamingNote, false);
                    localNote.ReplaceWith(roamingNote);
                    Debug.WriteLine("Roaming: Note updated");
                }
            }
        }

        public static void LoadNotesIfNecessary()
        {
            if (notes != null && notes.Count > 0) return;

            notes = LocalDB.GetAllWithChildren<Note>(x => x.IsArchived != true && x.DeletedAt == null).OrderByDescending(x => x.Order).ThenByDescending(x => x.CreatedAt).ToList();
            if (notes == null) notes = new Notes();
        }

        public static void LoadArchivedNotesIfNecessary()
        {
            if (archivedNotes != null && archivedNotes.Count > 0) return;

            archivedNotes = LocalDB.GetAllWithChildren<Note>(x => x.IsArchived == true && x.DeletedAt == null).OrderByDescending(x => x.ArchivedAt).ToList();
            if (archivedNotes == null) archivedNotes = new Notes();
        }
        
        public static Note TryGetNoteById(string id)
        {
            if(notes != null && notes.Count > 0)
            {
                var note = notes.FirstOrDefault(x => x.ID == id);
                if (note != null) return note;
            }

            try
            {
                return LocalDB.GetWithChildren<Note>(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<bool> CreateOrUpdateNote(Note note, bool reflectOnRoaming = true)
        {
            if (note == null) return false;

            bool alreadyExists = LocalDB.Find<Note>(note.ID) != null;
            
            if (note.IsEmpty())
                return await RemoveNote(note, reflectOnRoaming);
            else
                return alreadyExists ? UpdateNote(note, reflectOnRoaming) : CreateNote(note, reflectOnRoaming);
        }

        private static bool CreateNote(Note note, bool reflectOnRoaming = true)
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

            LocalDB.InsertWithChildren(note);
            if (reflectOnRoaming) RoamingDB.InsertWithChildren(note);

            var handler = NoteCreated;
            if (handler != null) handler(null, new NoteEventArgs(note));

            var handler2 = NotesSaved;
            if (handler2 != null) handler2(null, EventArgs.Empty);

            return true;
        }

        private static bool UpdateNote(Note note, bool reflectOnRoaming = true)
        {
            if (note == null) return false;

            Debug.WriteLine("Update note: " + note.Title);
            //App.TelemetryClient.TrackEvent("NoteUpdated");

            //associate with note
            foreach (var item in note.Checklist) item.NoteId = note.ID;
            foreach (var item in note.Images) item.NoteId = note.ID;

            //DB.UpdateWithChildren(note);
            bool success = LocalDB.Update(note) == 1;
            if (!success) return false;

            if(reflectOnRoaming) RoamingDB.Update(note);

            //update checklist items and note images
            if (note.Checklist.Count > 0)
            {
                LocalDB.InsertOrReplaceAll(note.Checklist);
                if (reflectOnRoaming) RoamingDB.InsertOrReplaceAll(note.Checklist);
            }

            if (note.Images.Count > 0)
            {
                LocalDB.InsertOrReplaceAll(note.Images);
                if (reflectOnRoaming) RoamingDB.InsertOrReplaceAll(note.Images);
            }

            note.Changed = false;

            var handler = NoteChanged;
            if (handler != null) handler(null, new NoteEventArgs(note));

            var handler2 = NotesSaved;
            if (handler2 != null) handler2(null, EventArgs.Empty);

            return true;
        }

        public static bool ArchiveNote(Note note, bool reflectOnRoaming = true)
        {
            //get original reference
            if(notes != null) note = Notes.FirstOrDefault(x => x.ID == note.ID);
            if (note == null) return false;

            Debug.WriteLine("Archive note: " + note.Title);
            //App.TelemetryClient.TrackEvent("NoteArchived");

            LoadArchivedNotesIfNecessary();
            note.IsArchived = true;
            note.TouchArchivedAt();

            bool success = LocalDB.Update(note) == 1;
            if (!success) return false;

            if (reflectOnRoaming) RoamingDB.Update(note);

            AppData.Notes.Remove(note);
            AppData.ArchivedNotes.Insert(0, note);
            Debug.WriteLine("AppData.ArchivedNotes.Insert(0, note);");

            var handler = NoteArchived;
            if (handler != null) handler(null, new NoteEventArgs(note));

            var handler2 = NotesSaved;
            if (handler2 != null) handler2(null, EventArgs.Empty);

            return true;
        }

        public static bool RestoreNote(Note note, bool reflectOnRoaming = true)
        {
            LoadArchivedNotesIfNecessary();

            //get original reference
            if(notes != null) note = ArchivedNotes.FirstOrDefault(x => x.ID == note.ID);
            if (note == null) return false;

            Debug.WriteLine("Restore note: " + note.Title);
            //App.TelemetryClient.TrackEvent("ArchivedNoteRestored");

            note.IsArchived = false;
            note.Order = Notes.Count;

            bool success = LocalDB.Update(note) == 1;
            if (!success) return false;

            if (reflectOnRoaming) RoamingDB.Update(note);

            AppData.Notes.Insert(0, note);
            AppData.ArchivedNotes.Remove(note);

            var handler = NoteRestored;
            if (handler != null) handler(null, new NoteEventArgs(note));

            var handler2 = NotesSaved;
            if (handler2 != null) handler2(null, EventArgs.Empty);

            return true;
        }

        public static bool ChangeNoteColor(Note note, NoteColor newColor)
        {
            if (note == null) return false;

            Debug.WriteLine("Change note color: " + newColor.Key);
            //App.TelemetryClient.TrackEvent("NoteColorChanged");

            note.Color = newColor;

            bool success = LocalDB.Update(note) == 1;
            if (!success) return false;
            RoamingDB.Update(note);

            var handler = NoteColorChanged;
            if (handler != null) handler(null, new NoteColorEventArgs(note, newColor));

            var handler2 = NotesSaved;
            if (handler2 != null) handler2(null, EventArgs.Empty);

            return true;
        }

        public static async Task<bool> RemoveNote(Note note, bool reflectOnRoaming = true)
        {
            //get original reference
            if (notes != null)
            {
                if (note.IsArchived)
                {
                    LoadArchivedNotesIfNecessary();
                    note = ArchivedNotes.FirstOrDefault(x => x.ID == note.ID);
                }
                else
                {
                    note = Notes.FirstOrDefault(x => x.ID == note.ID);
                }
            }

            if (note == null) return false;

            Debug.WriteLine("Remove note: " + note.Title);
            //App.TelemetryClient.TrackEvent("NoteRemoved");

            //remove note images from disk
            await RemoveNoteImages(note.Images);

            note.SoftDelete();

            bool success = LocalDB.Update(note) == 1;
            if (!success) return false;

            if(reflectOnRoaming) RoamingDB.Update(note);

            //LocalDB.Delete(note);
            //RoamingDB.Delete(note);

            AppData.Notes.Remove(note);
            AppData.ArchivedNotes.Remove(note);

            var handler = NoteRemoved;
            if (handler != null) handler(null, new NoteIdEventArgs(note.ID));

            var handler2 = NotesSaved;
            if (handler2 != null) handler2(null, EventArgs.Empty);

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
                {
                    LocalDB.Delete(noteImage);
                    RoamingDB.Delete(noteImage);
                }
            }
            catch (Exception)
            {
                success = false;
            }

            if(deleteFromDB)
            {
                var handler2 = NotesSaved;
                if (handler2 != null) handler2(null, EventArgs.Empty);
            }

            return success;
        }
    }
}