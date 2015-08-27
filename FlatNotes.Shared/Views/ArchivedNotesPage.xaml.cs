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
            var statusBarBackgroundColor = Color.FromArgb(0xff, 0x44, 0x59, 0x63);
            var statusBarForegroundColor = Color.FromArgb(0xD0, 0xff, 0xff, 0xff);
            App.ChangeStatusBarColor(statusBarBackgroundColor, statusBarForegroundColor);
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.RootFrame.Background = this.Background;
            if (viewModel.Notes == null || viewModel.Notes.Count <= 0) viewModel.Notes = AppData.ArchivedNotes;
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

            //this dispatcher fixes crash error (access violation on wp preview for developers)
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame.Navigate(typeof(NoteEditPage), note);
            });
        }
    }
}
