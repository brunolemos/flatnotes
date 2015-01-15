using System.Linq;
using Keep.Utils;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Resources;

namespace Keep.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            Themes = new ObservableCollection<ThemeModel>()
            {
                new ThemeModel() { Theme = ElementTheme.Light, Name = ResourceLoader.GetForCurrentView().GetString("Theme_Light") },
                new ThemeModel() { Theme = ElementTheme.Dark, Name = ResourceLoader.GetForCurrentView().GetString("Theme_Dark") }
            };
        }

        public ObservableCollection<ThemeModel> Themes { get; set; }
        public ThemeModel Theme { get { return Themes.Where( t => t.Theme == AppSettings.Instance.Theme).FirstOrDefault(); } set { AppSettings.Instance.Theme = value == null ? ElementTheme.Default : value.Theme; } }
        public int Columns { get { return AppSettings.Instance.Columns; } set { AppSettings.Instance.Columns = value; } }
    }

    public class ThemeModel
    {
        public string Name { get; set; }
        public ElementTheme Theme { get; set; }
    }
}