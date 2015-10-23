using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FlatNotes.Views
{
    public partial class MainPage : Page
    {
        public MainViewModel viewModel { get { return _viewModel; } }
        private static MainViewModel _viewModel = MainViewModel.Instance;

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        private Note RedirectToNote = null;

        public MainPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

#if WINDOWS_PHONE_APP
            Loaded += (s, e) => EnableReorderFeature();
            Unloaded += (s, e) => DisableReorderFeature();
#endif
        }

#if WINDOWS_PHONE_APP
        partial void EnableReorderFeature();
        partial void DisableReorderFeature();
#endif

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.ResetStatusBar();
            App.RootFrame.Background = this.Background;

            if (viewModel.Notes == null || viewModel.Notes.Count <= 0) viewModel.Notes = AppData.Notes;
            viewModel.ReorderMode = ListViewReorderMode.Disabled;

            //received a note via parameter (from secondary tile)
            if (RedirectToNote != null)
            {
                await Task.Delay(0200);
                Frame.Navigate(typeof(NoteEditPage), RedirectToNote);
                RedirectToNote = null;

                return;
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            RedirectToNote = null;
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //has parameters
            if (e.Parameter != null && !String.IsNullOrEmpty(e.Parameter.ToString()))
            {
                //note parameter
                RedirectToNote = e.NavigationMode == NavigationMode.New ? TileManager.TryToGetNoteFromNavigationArgument(e.Parameter.ToString()) : null;
            }

            Frame.BackStack.Clear();
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

#endregion
        
        private async void OnNoteClick(object sender, ItemClickEventArgs e)
        {
            Note note = e.ClickedItem as Note;
            if (note == null) return;

            //this dispatcher fixes crash error (access violation on wp preview for developers)
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame.Navigate(typeof(NoteEditPage), note);
            });
        }

        private void OnItemsReordered(object sender, Events.ItemsReorderedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("OnItemsReordered from {0} to {1}", e.OldItemIndex, e.NewItemIndex);
            if (e.OldItemIndex < 0 || e.NewItemIndex < 0) return;
            if (e.OldItemIndex > viewModel.Notes.Count || e.NewItemIndex > viewModel.Notes.Count) return;

            viewModel.Notes.Move(e.OldItemIndex, e.NewItemIndex);

            int pos = viewModel.Notes.Count - 1;
            foreach (var note in viewModel.Notes)
            {
                note.Order = pos;
                pos--;
            }

            AppData.LocalDB.UpdateAll(viewModel.Notes);
            AppData.RoamingDB.UpdateAll(viewModel.Notes);
        }

#if WINDOWS_PHONE_APP
        private void OnCreateNoteTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            viewModel.CreateTextNote();
        }
#endif
    }
}
