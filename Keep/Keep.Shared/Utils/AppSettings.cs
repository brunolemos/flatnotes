using Keep.Common;
using Keep.Events;
using Keep.Models;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Keep.Utils
{
    public class AppSettings : AppSettingsBase
    {
        public static readonly AppSettings Instance = new AppSettings();

        public event EventHandler ThemeChanged;

        private const string NOTES_FILENAME = "notes.json";
        private static Notes NOTES_DEFAULT = new Notes();

        private const string THEME_KEY = "THEME";
        private static ElementTheme THEME_DEFAULT = ElementTheme.Light;

        private const string COLUMNS_KEY = "COLUMNS";
        private static int COLUMNS_DEFAULT = 2;

        private AppSettings() : base() { }

        public ElementTheme Theme
        {
            get { return GetValueOrDefault(THEME_KEY, THEME_DEFAULT); }
            set {
                if (SetValue<ElementTheme>(THEME_KEY, value)) {
                    var handler = ThemeChanged;
                    if (handler != null) handler(this, EventArgs.Empty);
                }
            }
        }

        public int Columns
        {
            get { return GetValueOrDefault(COLUMNS_KEY, COLUMNS_DEFAULT); }
            set { SetValue<int>(COLUMNS_KEY, value); }
        }

        public async Task<Notes> LoadNotes() { return await ReadFileOrDefault(NOTES_FILENAME, NOTES_DEFAULT); }
        public async Task<bool> SaveNotes(Notes notes) { return await SaveFile(NOTES_FILENAME, notes); }
    }
}