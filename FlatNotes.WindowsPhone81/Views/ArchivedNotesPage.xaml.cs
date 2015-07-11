using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace FlatNotes.Views
{
    public partial class ArchivedNotesPage : Page
    {
        public ArchivedNotesViewModel viewModel { get { return _viewModel; } }
        private static ArchivedNotesViewModel _viewModel = new ArchivedNotesViewModel();

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        private static NoteSwipeFeature noteSwipeFeature = new NoteSwipeFeature();

        public ArchivedNotesPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            this.Loaded += (s, e) => App.ChangeStatusBarColor(Color.FromArgb(0xff, 0x44, 0x59, 0x63), Color.FromArgb(0xff, 0xff, 0xff, 0xfe));
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("ArchivedNotesPage");
            
            App.RootFrame.Background = LayoutRoot.Background;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SetCustomMetric(1, AppData.Notes.Count);
            GoogleAnalytics.EasyTracker.GetTracker().SetCustomMetric(2, AppData.ArchivedNotes.Count);
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

        private async void OnNoteTapped(object sender, TappedRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if (viewModel.ReorderMode == ListViewReorderMode.Enabled) return;
#endif

            Note note = (e.OriginalSource as FrameworkElement).DataContext as Note;
            if (note == null) return;

            //it can be trimmed, so get the original
            Note originalNote = AppData.ArchivedNotes.Where<Note>(n => n.ID == note.ID).FirstOrDefault();
            if (originalNote == null)
            {
                GoogleAnalytics.EasyTracker.GetTracker().SendException(string.Format("Failed to load tapped archived note ({0})", Newtonsoft.Json.JsonConvert.SerializeObject(AppData.ArchivedNotes)), false);
                return;
            }

            //this dispatcher fixes crash error (access violation on wp preview for developers)
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame.Navigate(typeof(NoteEditPage), originalNote);
            });
        }

        //swipe feature
        private void OnNoteLoaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            FrameworkElement element = sender as FrameworkElement;
            FrameworkElement referenceFrame = NotesListView;

            if (viewModel.ReorderMode != ListViewReorderMode.Enabled)
                noteSwipeFeature.EnableSwipeFeature(element, referenceFrame);

            noteSwipeFeature.enableSwipeEventHandlers[element] = (s, _e) => { noteSwipeFeature.EnableSwipeFeature(element, referenceFrame); };
            noteSwipeFeature.disableSwipeEventHandlers[element] = (s, _e) => { noteSwipeFeature.DisableSwipeFeature(element); };

            if (noteSwipeFeature.enableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeDisabled += noteSwipeFeature.enableSwipeEventHandlers[element];
            if (noteSwipeFeature.disableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeEnabled += noteSwipeFeature.disableSwipeEventHandlers[element];
#endif
        }

        private void OnNoteUnloaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            FrameworkElement element = sender as FrameworkElement;

            if (noteSwipeFeature.enableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeDisabled -= noteSwipeFeature.enableSwipeEventHandlers[element];
            if (noteSwipeFeature.disableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeEnabled -= noteSwipeFeature.disableSwipeEventHandlers[element];

            noteSwipeFeature.enableSwipeEventHandlers.Remove(element);
            noteSwipeFeature.disableSwipeEventHandlers.Remove(element);

            noteSwipeFeature.DisableSwipeFeature(element);
#endif
        }
    }
}
