﻿using FlatNotes.Common;
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

#if WINDOWS_PHONE_APP
        private static NoteSwipeFeature noteSwipeFeature = new NoteSwipeFeature();
#endif

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

            Loaded += OnLoaded;
        }

#if WINDOWS_PHONE_APP
        partial void EnableReorderFeature();
        partial void DisableReorderFeature();
#endif

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            App.ResetStatusBar();

        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.RootFrame.Background = this.Background;
            if (viewModel.Notes == null || viewModel.Notes.Count <= 0) viewModel.Notes = AppData.Notes;

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

#if WINDOWS_UWP
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

#elif WINDOWS_PHONE_APP
        private async void OnNoteTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (viewModel.ReorderMode == ListViewReorderMode.Enabled) return;

            Note note = (sender as FrameworkElement).DataContext as Note;
            if (note == null) return;

            //this dispatcher fixes crash error (access violation on wp preview for developers)
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame.Navigate(typeof(NoteEditPage), note);
            });
        }

        //swipe feature
        private void OnNoteLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FrameworkElement referenceFrame = NotesControl;

            if(viewModel.ReorderMode != ListViewReorderMode.Enabled)
                noteSwipeFeature.EnableSwipeFeature(element, referenceFrame);

            noteSwipeFeature.enableSwipeEventHandlers[element] = (s, _e) => { noteSwipeFeature.EnableSwipeFeature(element, referenceFrame); };
            noteSwipeFeature.disableSwipeEventHandlers[element] = (s, _e) => { noteSwipeFeature.DisableSwipeFeature(element); };

            viewModel.ReorderModeDisabled += noteSwipeFeature.enableSwipeEventHandlers[element];
            viewModel.ReorderModeEnabled += noteSwipeFeature.disableSwipeEventHandlers[element];
        }

        private void OnNoteUnloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if (noteSwipeFeature.enableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeDisabled -= noteSwipeFeature.enableSwipeEventHandlers[element];
            if (noteSwipeFeature.disableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeEnabled -= noteSwipeFeature.disableSwipeEventHandlers[element];

            noteSwipeFeature.enableSwipeEventHandlers.Remove(element);
            noteSwipeFeature.disableSwipeEventHandlers.Remove(element);

            noteSwipeFeature.DisableSwipeFeature(element);
        }
#endif

        private void OnItemsReordered(object sender, Events.ItemsReorderedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnItemsReordered from {0} to {1}", e.OldItemIndex, e.NewItemIndex);
            if (e.OldItemIndex < 0 || e.NewItemIndex < 0) return;
            if (e.OldItemIndex > viewModel.Notes.Count || e.NewItemIndex > viewModel.Notes.Count) return;

            viewModel.Notes.Move(e.OldItemIndex, e.NewItemIndex);

            int pos = viewModel.Notes.Count - 1;
            foreach (var note in viewModel.Notes)
            {
                note.Order = pos;
                pos--;
            }

            AppData.DB.UpdateAll(viewModel.Notes);
        }
    }
}