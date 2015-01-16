using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Keep.Views
{
    public sealed partial class MainPage : Page
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public MainViewModel viewModel { get { return (MainViewModel)DataContext; } }

        public MainPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            this.Loaded += (s, e) => EnableReorderFeature();
            this.Unloaded += (s, e) => DisableReorderFeature();
        }

        partial void EnableReorderFeature();
        partial void DisableReorderFeature();

        partial void EnableSwipeFeature(FrameworkElement element, FrameworkElement referenceFrame);
        partial void DisableSwipeFeature(FrameworkElement element);

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.ChangeStatusBarColor();
            App.RootFrame.Background = LayoutRoot.Background;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }


        #endregion

        private void OnNoteTapped(object sender, TappedRoutedEventArgs e)
        {
            #if WINDOWS_PHONE_APP
            if (viewModel.ReorderMode == ListViewReorderMode.Enabled) return;
            #endif

            Note note = (e.OriginalSource as FrameworkElement).DataContext as Note;
            if (note == null) return;

            //it can be trimmed, so get the original
            if (note.IsChecklist)
                note = AppData.Notes.Where<Note>(n => n.ID == note.ID).FirstOrDefault();

            App.RootFrame.Navigate(typeof(NoteEditPage), note);
        }

        //swipe feature
        private void OnNoteLoaded(object sender, RoutedEventArgs e)
        {
            EnableSwipeFeature(sender as FrameworkElement, NotesListView);

            viewModel.ReorderModeDisabled += (s, _e) => { EnableSwipeFeature(sender as FrameworkElement, NotesListView); };
            viewModel.ReorderModeEnabled += (s, _e) => { DisableSwipeFeature(sender as FrameworkElement); };
        }

        private void OnNoteUnloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
