using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace FlatNotes.Views
{
    public sealed partial class MainPage : Page
    {
        partial void EnableReorderFeature()
        {
            navigationHelper.LoadState += NavigationHelper_LoadState1;
            navigationHelper.SaveState += NavigationHelper_SaveState1;

            if(NotesControl != null)
            {
                NotesControl.Holding += NotesListView_Holding;
                NotesControl.DragItemsStarting += NotesListView_DragItemsStarting;
            }
        }

        partial void DisableReorderFeature()
        {
            navigationHelper.LoadState -= NavigationHelper_LoadState1;
            navigationHelper.SaveState -= NavigationHelper_SaveState1;

            if (NotesControl != null)
            {
                NotesControl.Holding -= NotesListView_Holding;
                NotesControl.DragItemsStarting -= NotesListView_DragItemsStarting;
            }

        }

        private void NavigationHelper_LoadState1(object sender, LoadStateEventArgs e)
        {
            NotesControl.ReorderMode = ListViewReorderMode.Disabled;
            viewModel.ReorderedNotes = false;
        }

        private void NavigationHelper_SaveState1(object sender, SaveStateEventArgs e)
        {
            NotesControl.ReorderMode = ListViewReorderMode.Disabled;
        }

        private void NotesListView_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            NotesControl.ReorderMode = NotesControl.Items.Count > 1 ? ListViewReorderMode.Enabled : ListViewReorderMode.Disabled;
        }

        private void NotesListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            e.Data.Properties["sourceItem"] = e.Items[0];
        }
    }
}
