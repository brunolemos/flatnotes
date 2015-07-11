using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using FlatNotes.Common;
using FlatNotes.ViewModels;
using FlatNotes.Utils;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;

namespace FlatNotes
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel viewModel { get { return _viewModel; } }
        private static SettingsViewModel _viewModel = new SettingsViewModel();

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public SettingsPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("SettingsPage");
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

        //private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if ((e.AddedItems[0] as PivotItem).Tag.ToString() == "about")
        //        CommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
        //    else
        //        CommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
        //}
    }
}
