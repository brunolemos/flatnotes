using System.Linq;
using Keep.Utils;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace Keep.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            Themes = new ObservableCollection<ThemeModel>()
            {
                new ThemeModel() { Name = "Light", Theme = ElementTheme.Light },
                new ThemeModel() { Name = "Dark", Theme = ElementTheme.Dark }
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