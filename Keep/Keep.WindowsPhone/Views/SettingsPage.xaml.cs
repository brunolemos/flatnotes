using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Keep.Common;
using Keep.ViewModels;
using Keep.Utils;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;

namespace Keep
{
    public sealed partial class SettingsPage : Page
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public SettingsViewModel viewModel { get { return (SettingsViewModel)DataContext; } }

        public SettingsPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            AppSettings.Instance.ThemeChanged += FixComboBoxTheme;
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.ChangeStatusBarColor();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void FixComboBoxTheme(object sender, Events.ThemeEventArgs e)
        {
            if (e.Theme == ElementTheme.Light)
            {
                ThemeComboBox.BorderBrush = new SolidColorBrush(Color.FromArgb(0xC0, 0x00, 0x00, 0x00));
                ThemeComboBox.Foreground = new SolidColorBrush(Color.FromArgb(0xC0, 0x00, 0x00, 0x00));
            } else
            {
                ThemeComboBox.BorderBrush = new SolidColorBrush(Colors.White);
                ThemeComboBox.Foreground = new SolidColorBrush(Colors.White);
            }
        }
    }
}
