using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;
using System.Linq;
using Windows.UI.Xaml.Controls;
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

            EnableReorder();
        }

        partial void EnableReorder();

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


        private void NotesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            #if WINDOWS_PHONE_APP
            if (NotesListView.ReorderMode == ListViewReorderMode.Enabled) return;
            #endif

            Note note = e.ClickedItem as Note;
            if (note == null) return;

            //it can be trimmed, so get the original
            if(note.IsChecklist)
                note = AppData.Notes.Where<Note>(n => n.ID == note.ID).FirstOrDefault();

            Frame.Navigate(typeof(NoteEditPage), note);
        }
    }
}
