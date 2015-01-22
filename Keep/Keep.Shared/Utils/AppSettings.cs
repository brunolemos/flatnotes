using Keep.Common;
using Keep.Events;
using Keep.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Keep.Utils
{
    public class AppSettings : AppSettingsBase
    {
        public static readonly AppSettings Instance = new AppSettings();

        public event EventHandler<ThemeEventArgs> ThemeChanged;
        public event EventHandler ColumnsChanged;

        public override uint Version { get { return version; } }
        private static uint version = 2;

        public StorageFolder ImagesFolder { get; private set; }
        private const string IMAGES_FOLDER_NAME = "Images";

        private const string NOTES_FILENAME = "notes.json";
        private static Notes NOTES_DEFAULT = new Notes();

        private const string ARCHIVED_NOTES_FILENAME = "notes_archived.json";
        private static Notes ARCHIVED_NOTES_DEFAULT = new Notes();

        private const string THEME_KEY = "THEME";
        private static ElementTheme THEME_DEFAULT = ElementTheme.Dark;

        private const string COLUMNS_KEY = "COLUMNS";
        private static int COLUMNS_DEFAULT = 2;

        private AppSettings()
        {
            Initialize();
        }

        private async void Initialize()
        {
            if (DesignMode.DesignModeEnabled) return;
            ImagesFolder = await localFolder.CreateFolderAsync(IMAGES_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
        }

        public override void Up()
        {
            if (Migration.Versions.v1.AppSettings.Instance.LoggedUser == Migration.Versions.v1.AppSettings.Instance.LOGGEDUSER_DEFAULT)
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
            }).Wait();

            Theme = Migration.Versions.v1.AppSettings.Instance.LoggedUser.Preferences.Theme;
            Columns = Migration.Versions.v1.AppSettings.Instance.LoggedUser.Preferences.Columns;
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
            set {
                if (value != ElementTheme.Light && value != ElementTheme.Dark) value = THEME_DEFAULT;

                if (SetValue<ElementTheme>(THEME_KEY, value)) {
                    var handler = ThemeChanged;
                    if (handler != null) handler(this, new ThemeEventArgs(value));
                }
            }
        }

        public int Columns
        {
            get { return GetValueOrDefault(COLUMNS_KEY, COLUMNS_DEFAULT); }
            set
            {
                if (SetValue<int>(COLUMNS_KEY, value))
                {
                    var handler = ColumnsChanged;
                    if (handler != null) handler(this, EventArgs.Empty);
                }
            }
        }

        public async Task<Notes> LoadNotes() { return await ReadFileOrDefault(NOTES_FILENAME, NOTES_DEFAULT); }
        public async Task<bool> SaveNotes(Notes notes) { return await SaveFile(NOTES_FILENAME, notes); }

        public async Task<Notes> LoadArchivedNotes() { return await ReadFileOrDefault(ARCHIVED_NOTES_FILENAME, ARCHIVED_NOTES_DEFAULT); }
        public async Task<bool> SaveArchivedNotes(Notes notes) { return await SaveFile(ARCHIVED_NOTES_FILENAME, notes); }

        public async Task<StorageFile> SaveImage(StorageFile file, string noteId, string noteImageId)
        {
            string newFileName = String.Format("{0}_{1}{2}", noteId, noteImageId, file.FileType);

            return await file.CopyAsync(ImagesFolder, newFileName, NameCollisionOption.ReplaceExisting);
        }
    }
}