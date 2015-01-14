using Keep.Models;
using Keep.Utils;
using Windows.UI.Xaml;

namespace Keep.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel() { }

        public ElementTheme Theme { get { return AppSettings.Instance.Theme; } set { AppSettings.Instance.Theme = value; } }
        public int Columns { get { return AppSettings.Instance.Columns; } set { AppSettings.Instance.Columns = value; } }
    }
}
