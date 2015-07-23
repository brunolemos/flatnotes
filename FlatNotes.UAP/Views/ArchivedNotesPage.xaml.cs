using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FlatNotes.Views
{
    public partial class ArchivedNotesPage : Page
    {
        public ArchivedNotesViewModel viewModel { get { return _viewModel; } }
        private static ArchivedNotesViewModel _viewModel = ArchivedNotesViewModel.Instance;

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public ArchivedNotesPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            App.ChangeStatusBarColor(Color.FromArgb(0xff, 0x44, 0x59, 0x63), Color.FromArgb(0xff, 0xff, 0xff, 0xfe));

            if (viewModel.IsLoaded)
                return;

            viewModel.IsLoading = true;
            viewModel.Notes = AppData.ArchivedNotes;

            viewModel.IsLoaded = true;
            viewModel.IsLoading = false;
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.RootFrame.Background = LayoutRoot.Background;
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

        private async void OnNoteClick(object sender, ItemClickEventArgs e)
        {
            Note note = e.ClickedItem as Note;
            if (note == null) return;

            ////it can be trimmed, so get the original
            //Note originalNote = AppData.DB.GetWithChildren<Note>(note.ID);
            //if (originalNote == null)
            //{
            //    var exceptionProperties = new Dictionary<string, string>() { { "Details", "Failed to load tapped archived note" }, { "id", note.ID } };
            //    App.TelemetryClient.TrackException(null, exceptionProperties);
            //    return;
            //}

            //this dispatcher fixes crash error (access violation on wp preview for developers)
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame.Navigate(typeof(NoteEditPage), note);
            });
        }
    }
}
