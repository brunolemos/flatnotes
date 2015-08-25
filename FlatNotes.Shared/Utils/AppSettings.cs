using FlatNotes.Common;
using FlatNotes.Events;
using FlatNotes.Models;
using SQLiteNetExtensions.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;

namespace FlatNotes.Utils
{
    public class AppSettings : AppSettingsBase
    {
        public static AppSettings Instance { get { if (instance == null) instance = new AppSettings(); return instance; } }
        private static AppSettings instance;

        public event EventHandler<ThemeEventArgs> ThemeChanged;
        public event EventHandler<IsSingleColumnEnabledEventArgs> IsSingleColumnEnabledChanged;
        public event EventHandler<TransparentTileEventArgs> TransparentTileChanged;
        public event EventHandler<TransparentTileEventArgs> TransparentNoteTileChanged;

        public override uint Version { get { return VERSION; } }
        private const uint VERSION = 3;

        public StorageFolder ImagesFolder { get; private set; }
        private const string IMAGES_FOLDER_NAME = "Images";

        private const string THEME_KEY = "THEME";
        private const ElementTheme THEME_DEFAULT = ElementTheme.Light;

        private const string IS_SINGLE_COLUMN_ENABLED_KEY = "IS_SINGLE_COLUMN_ENABLED";
        private const bool IS_SINGLE_COLUMN_ENABLED_DEFAULT = false;

        private const string TRANSPARENT_TILE_KEY = "TRANSPARENT_TILE";
        private const bool TRANSPARENT_TILE_DEFAULT = false;

        private const string TRANSPARENT_NOTE_TILE_KEY = "TRANSPARENT_NOTE_TILE";
        private const bool TRANSPARENT_NOTE_TILE_DEFAULT = false;

        private const string FIXED_THEME_BUG_KEY = "FIXED_THEME_BUG";

        private AppSettings()
        {
            Initialize();
        }

        private async void Initialize()
        {
            if (DesignMode.DesignModeEnabled) return;
            ImagesFolder = await localFolder.CreateFolderAsync(IMAGES_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
        }

        /// <summary>
        /// Migrate from v2 to v3
        /// </summary>
        public static void Up()
        {
            Task.Run(async () =>
            {
                //import notes
                Notes allNotes = new Notes();
                var notes = await Migration.Versions.v2.Utils.AppSettings.Instance.LoadNotes();
                if(notes != null && notes.Count > 0)
                {
                    for (int i = 0; i < notes.Count - 1; i++)
                    {
                        var note = (Note)notes[i];
                        note.Order = notes.Count - 1 - i;
                        allNotes.Add(note);
                    }
                }

                Notes archivedNotes = await Migration.Versions.v2.Utils.AppSettings.Instance.LoadArchivedNotes();
                if (allNotes.Count <= 0 && (archivedNotes == null || archivedNotes.Count <= 0)) return;

                //merge archived notes
                if(archivedNotes != null)
                    foreach (var note in archivedNotes)
                    {
                        note.IsArchived = true;
                        allNotes.Add(note);
                    }

                foreach (var note in allNotes)
                {
                    if (note.Checklist != null && note.Checklist.Count > 0)
                        foreach (var checklistItem in note.Checklist)
                            checklistItem.NoteId = note.ID;

                    if (note.Images != null && note.Images.Count > 0)
                        foreach (var noteImage in note.Images)
                            noteImage.NoteId = note.ID;
                }

                //insert in db
                AppData.DB.InsertOrReplaceAllWithChildren(allNotes);

                allNotes = null;
            }).Wait();
        }

        /// <summary>
        /// Migrate from v3 back to v2
        /// </summary>
        public static void Down()
        {
            Task.Run(async () =>
            {
                //import notes
                Notes notes = AppData.DB.GetAllWithChildren<Note>(x => x.IsArchived != true).OrderByDescending(x => x.Order).ToList();
                Notes archivedNotes = AppData.DB.GetAllWithChildren<Note>(x => x.IsArchived == true).OrderByDescending(x => x.ArchivedAt).ToList();

                bool success = await Migration.Versions.v2.Utils.AppSettings.Instance.SaveNotes(notes);
                success &= await Migration.Versions.v2.Utils.AppSettings.Instance.SaveArchivedNotes(archivedNotes);

                //if (success) AppData.DB.DeleteAll<Note>();
            }).Wait();
        }

        public ElementTheme Theme
        {
            get { return GetValueOrDefault(THEME_KEY, THEME_DEFAULT); }
            set
            {
                if (value != ElementTheme.Light && value != ElementTheme.Dark) value = THEME_DEFAULT;

                if (SetValue<ElementTheme>(THEME_KEY, value)) {
                    App.TelemetryClient.TrackMetric("Theme", (int)value);

                    var handler = ThemeChanged;
                    if (handler != null) handler(this, new ThemeEventArgs(value));
                }
            }
        }

        public bool IsSingleColumnEnabled
        {
            get { return GetValueOrDefault(IS_SINGLE_COLUMN_ENABLED_KEY, IS_SINGLE_COLUMN_ENABLED_DEFAULT); }
            set
            {
                if (SetValue<bool>(IS_SINGLE_COLUMN_ENABLED_KEY, value))
                {
                    App.TelemetryClient.TrackMetric("Single Column", value ? 1 : 0);

                    var handler = IsSingleColumnEnabledChanged;
                    if (handler != null) handler(this, new IsSingleColumnEnabledEventArgs(value));
                }
            }
        }

        public bool TransparentTile
        {
            get { return GetValueOrDefault(TRANSPARENT_TILE_KEY, TRANSPARENT_TILE_DEFAULT); }
            set
            {
                if (SetValue<bool>(TRANSPARENT_TILE_KEY, value))
                {
                    App.TelemetryClient.TrackMetric("Transparent Tile", value ? 1 : 0);

                    var handler = TransparentTileChanged;
                    if (handler != null) handler(this, new TransparentTileEventArgs(value));
                }
            }
        }

        public bool TransparentNoteTile
        {
            get { return GetValueOrDefault(TRANSPARENT_NOTE_TILE_KEY, TRANSPARENT_NOTE_TILE_DEFAULT); }
            set
            {
                if (SetValue<bool>(TRANSPARENT_NOTE_TILE_KEY, value))
                {
                    App.TelemetryClient.TrackMetric("Transparent Note Tile", value ? 1 : 0);

                    var handler = TransparentNoteTileChanged;
                    if (handler != null) handler(this, new TransparentTileEventArgs(value));
                }
            }
        }

        public async Task<StorageFile> SaveImage(StorageFile file, string noteId, string noteImageId)
        {
            string newFileName = String.Format("{0}_{1}{2}", noteId, noteImageId, file.FileType);

            return await file.CopyAsync(ImagesFolder, newFileName, NameCollisionOption.ReplaceExisting);
        }
    }
}