using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.Views
{
    public sealed partial class MainPage : Page
    {
        partial void EnableReorderFeature()
        {
            navigationHelper.LoadState += NavigationHelper_LoadState1;
            navigationHelper.SaveState += NavigationHelper_SaveState1;

            NotesControl.Holding += NotesListView_Holding;
            NotesControl.DragOver += NotesListView_DragOver;
            NotesControl.Drop += NotesListView_Drop;
            viewModel.ReorderModeDisabled += OnReorderModeDisabled;

#if WINDOWS_PHONE_APP
            NotesControl.DragItemsStarting += NotesListView_DragItemsStarting;
#endif
        }

        partial void DisableReorderFeature()
        {
            navigationHelper.LoadState -= NavigationHelper_LoadState1;
            navigationHelper.SaveState -= NavigationHelper_SaveState1;

            if (NotesControl != null)
            {
                NotesControl.Holding -= NotesListView_Holding;
                NotesControl.DragOver -= NotesListView_DragOver;
                NotesControl.Drop -= NotesListView_Drop;
                viewModel.ReorderModeDisabled -= OnReorderModeDisabled;
            }

#if WINDOWS_PHONE_APP
                NotesControl.DragItemsStarting -= NotesListView_DragItemsStarting;
#endif
        }

        private void NavigationHelper_LoadState1(object sender, LoadStateEventArgs e)
        {
            #if WINDOWS_PHONE_APP
            NotesControl.ReorderMode = ListViewReorderMode.Disabled;
            #endif

            viewModel.ReorderedNotes = false;
        }

        private void NavigationHelper_SaveState1(object sender, SaveStateEventArgs e)
        {
            #if WINDOWS_PHONE_APP
            NotesControl.ReorderMode = ListViewReorderMode.Disabled;
            #endif
        }

        private void OnReorderModeDisabled(object sender, System.EventArgs e)
        {
            SaveNotesIfReordered();
        }

        private void SaveNotesIfReordered()
        {
            Debug.WriteLine("SaveNotesIfReordered -- TODO");

            ////save notes if reordered
            //if (viewModel.ReorderedNotes)
            //    if (await AppData.SaveNotes())
            //        viewModel.ReorderedNotes = false;
        }

        private void NotesListView_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            #if WINDOWS_PHONE_APP
            NotesControl.ReorderMode = NotesControl.Items.Count > 1 ? ListViewReorderMode.Enabled : ListViewReorderMode.Disabled;
            #endif
        }

#if WINDOWS_PHONE_APP
        private void NotesListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            e.Data.Properties["sourceItem"] = e.Items[0];
        }
#endif

        private void NotesListView_DragOver(object sender, DragEventArgs e)
        {
            e.Data.Properties["targetItem"] = (e.OriginalSource as FrameworkElement).DataContext;
        }

        private void NotesListView_Drop(object sender, DragEventArgs e)
        {
            var sourceItem = e.Data.Properties["sourceItem"] as Note;
            var targetItem = e.Data.Properties["targetItem"] as Note;
            if (sourceItem == null || targetItem == null) return;

            var items = (sender as ItemsControl).ItemsSource as Notes;
            if (items == null) return;

            int sourceItemIndex = items.IndexOf(sourceItem);
            int targetItemIndex = items.IndexOf(targetItem);
            if (sourceItemIndex == targetItemIndex || sourceItemIndex < 0 || targetItemIndex < 0) return;

            viewModel.ReorderedNotes = true;
            items.Move(sourceItemIndex, targetItemIndex);
        }
    }
}
