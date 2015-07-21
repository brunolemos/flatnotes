using FlatNotes.Common;
using FlatNotes.Events;
using FlatNotes.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace FlatNotes.Utils
{
    public class AppSettings : AppSettingsBase
    {
        public static readonly AppSettings Instance = new AppSettings();

        public event EventHandler NotesSaved;
        public event EventHandler ArchivedNotesSaved;
        public event EventHandler<ThemeEventArgs> ThemeChanged;
        //public event EventHandler<ColumnsEventArgs> ColumnsChanged;
        public event EventHandler<IsSingleColumnEnabledEventArgs> IsSingleColumnEnabledChanged;
        public event EventHandler<TransparentTileEventArgs> TransparentTileChanged;
        public event EventHandler<TransparentTileEventArgs> TransparentNoteTileChanged;

        public override uint Version { get { return VERSION; } }
        private const uint VERSION = 2;

        public StorageFolder ImagesFolder { get; private set; }
        private const string IMAGES_FOLDER_NAME = "Images";

        private const string NOTES_FILENAME = "notes.json";
        private static Notes NOTES_DEFAULT = new Notes();

        private const string ARCHIVED_NOTES_FILENAME = "notes_archived.json";
        private static Notes ARCHIVED_NOTES_DEFAULT = new Notes();

        private const string THEME_KEY = "THEME";
        private const ElementTheme THEME_DEFAULT = ElementTheme.Light;

        //private const string COLUMNS_KEY = "COLUMNS";
        //private const int COLUMNS_DEFAULT = -1;

        private const string IS_SINGLE_COLUMN_ENABLED_KEY = "IS_SINGLE_COLUMN_ENABLED";
        private const bool IS_SINGLE_COLUMN_ENABLED_DEFAULT = false;

        private const string TRANSPARENT_TILE_KEY = "TRANSPARENT_TILE";
        private const bool TRANSPARENT_TILE_DEFAULT = false;

        private const string TRANSPARENT_NOTE_TILE_KEY = "TRANSPARENT_NOTE_TILE";
        private const bool TRANSPARENT_NOTE_TILE_DEFAULT = false;

        //private const string DISABLE_ANALYTICS_KEY = "DISABLE_ANALYTICS";
        //private const bool DISABLE_ANALYTICS_DEFAULT = false;

        private const string FIXED_THEME_BUG_KEY = "FIXED_THEME_BUG";

        private AppSettings()
        {
            Initialize();
        }

        private async void Initialize()
        {
            if (DesignMode.DesignModeEnabled) return;
            ImagesFolder = await localFolder.CreateFolderAsync(IMAGES_FOLDER_NAME, CreationCollisionOption.OpenIfExists);

            //fix theme bug
            if (localSettings.Values[THEME_KEY] == null || localSettings.Values[THEME_KEY].ToString().Length > 1 || localSettings.Values[THEME_KEY].ToString() == "0")
                Theme = THEME_DEFAULT;
        }

        public override void Up()
        {
            if (Migration.Versions.v1.AppSettings.Instance.LoggedUser.Notes.Count <= 0)
                return;

            //import notes
            Notes notes = new Notes();
            foreach (var note in Migration.Versions.v1.AppSettings.Instance.LoggedUser.Notes)
                notes.Add(new Note(note.Title, note.Text, note.Checklist, note.Color, note.CreatedAt, note.UpdatedAt, null));

            //import archived
            Notes archivedNotes = new Notes();
            foreach (var note in Migration.Versions.v1.AppSettings.Instance.LoggedUser.ArchivedNotes)
                notes.Add(new Note(note.Title, note.Text, note.Checklist, note.Color, note.CreatedAt, note.UpdatedAt, note.UpdatedAt));

            Task.Run(async () =>
            {
                bool success = await SaveNotes(notes);
                await SaveArchivedNotes(archivedNotes);

                if (success) localSettings.Values.Clear();

                Theme = Migration.Versions.v1.AppSettings.Instance.LoggedUser.Preferences.Theme;
                //Columns = Migration.Versions.v1.AppSettings.Instance.LoggedUser.Preferences.Columns;
            }).Wait();
        }

        public override void Down()
        {
            Task.Run(async () =>
            {
                Migration.Versions.v1.AppSettings.Instance.LoggedUser.Preferences.Theme = Theme;
                Migration.Versions.v1.AppSettings.Instance.LoggedUser.Preferences.Columns = Columns;
                Migration.Versions.v1.AppSettings.Instance.LoggedUser.Notes = await LoadNotes();
                Migration.Versions.v1.AppSettings.Instance.LoggedUser.ArchivedNotes = await LoadArchivedNotes();
            }).Wait();
        }

        public ElementTheme Theme
        {
            get { return GetValueOrDefault(THEME_KEY, THEME_DEFAULT); }
            set
            {
                if (value != ElementTheme.Light && value != ElementTheme.Dark) value = THEME_DEFAULT;

                if (SetValue<ElementTheme>(THEME_KEY, value)) {
                    var handler = ThemeChanged;
                    if (handler != null) handler(this, new ThemeEventArgs(value));

                    App.TelemetryClient.TrackMetric("Theme", value == ElementTheme.Light ? 1 : 2);
                }
            }
        }

        public int Columns
        {
            get { return -1; /*GetValueOrDefault(COLUMNS_KEY, COLUMNS_DEFAULT);*/ }
            //set
            //{
            //    if (SetValue<int>(COLUMNS_KEY, value))
            //    {
            //        var handler = ColumnsChanged;
            //        if (handler != null) handler(this, new ColumnsEventArgs(value));

            //        App.TelemetryClient.TrackMetric("Columns", value);
            //    }
            //}
        }

        public bool IsSingleColumnEnabled
        {
            get { return GetValueOrDefault(IS_SINGLE_COLUMN_ENABLED_KEY, IS_SINGLE_COLUMN_ENABLED_DEFAULT); }
            set
            {
                if (SetValue<bool>(IS_SINGLE_COLUMN_ENABLED_KEY, value))
                {
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
                    var handler = TransparentNoteTileChanged;
                    if (handler != null) handler(this, new TransparentTileEventArgs(value));
                }
            }
        }

        //public bool DisableAnalytics
        //{
        //    get { return GetValueOrDefault(DISABLE_ANALYTICS_KEY, DISABLE_ANALYTICS_DEFAULT, true); }
        //    set { SetValue<bool>(DISABLE_ANALYTICS_KEY, value, true); }
        //}

        public async Task<Notes> LoadNotes() { return await ReadFileOrDefault(NOTES_FILENAME, NOTES_DEFAULT); }
        public async Task<bool> SaveNotes(Notes notes) {
            var success = await SaveFile(NOTES_FILENAME, notes);

            if(success)
            {
                var handler = NotesSaved;
                if (handler != null) handler(this, EventArgs.Empty);
            }

            return success;
        }

        public async Task<Notes> LoadArchivedNotes() { return await ReadFileOrDefault(ARCHIVED_NOTES_FILENAME, ARCHIVED_NOTES_DEFAULT); }
        public async Task<bool> SaveArchivedNotes(Notes notes)
        {
            var success = await SaveFile(ARCHIVED_NOTES_FILENAME, notes);

            if (success)
            {
                var handler = ArchivedNotesSaved;
                if (handler != null) handler(this, EventArgs.Empty);
            }

            return success;
        }

        public async Task<StorageFile> SaveImage(StorageFile file, string noteId, string noteImageId)
        {
            string newFileName = String.Format("{0}_{1}{2}", noteId, noteImageId, file.FileType);

            return await file.CopyAsync(ImagesFolder, newFileName, NameCollisionOption.ReplaceExisting);
        }
    }
}