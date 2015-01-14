using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Keep.Common;
using Keep.ViewModels;
using Keep.Models;

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

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }


        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //App.ChangeStatusBarColor();
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
    }
}
