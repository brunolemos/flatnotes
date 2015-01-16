using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;
using System.Linq;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
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

            HardwareButtons.BackPressed += (s, e) => SaveNotesIfReordered();
            App.Current.Suspending += (s, e) => SaveNotesIfReordered();
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.ChangeStatusBarColor();
            App.RootFrame.Background = LayoutRoot.Background;

            viewModel.ReorderedNotes = false;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            #if WINDOWS_PHONE_APP
            NotesListView.ReorderMode = ListViewReorderMode.Disabled;
            #endif

            SaveNotesIfReordered();
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

        private async void SaveNotesIfReordered()
        {
            //save notes if reordered
            if (viewModel.ReorderedNotes)
                if (await AppData.SaveNotes())
                    viewModel.ReorderedNotes = false;
        }

        private void NotesListView_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            #if WINDOWS_PHONE_APP
            NotesListView.ReorderMode = ListViewReorderMode.Enabled;
            #endif
        }

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

        private void NotesListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            e.Data.Properties["noteSource"] = e.Items[0];
        }

        private void NotesListView_DragOver(object sender, DragEventArgs e)
        {
            e.Data.Properties["noteTarget"] = (e.OriginalSource as FrameworkElement).DataContext;
        }

        private void NotesListView_Drop(object sender, DragEventArgs e)
        {
            Note noteSource = e.Data.Properties["noteSource"] as Note;
            Note noteTarget = e.Data.Properties["noteTarget"] as Note;
            if (noteSource == null || noteTarget == null) return;

            Notes notes = (sender as ListView).ItemsSource as Notes;
            if (notes == null) return;

            int noteSourceIndex = notes.IndexOf(noteSource);
            int noteTargetIndex = notes.IndexOf(noteTarget);
            if (noteSourceIndex == noteTargetIndex || noteSourceIndex < 0 || noteTargetIndex < 0) return;

            viewModel.ReorderedNotes = true;
            notes.Move(noteSourceIndex, noteTargetIndex);
        }
    }
}
