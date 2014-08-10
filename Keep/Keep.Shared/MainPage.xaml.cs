using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Keep.Common;
using Keep.Commands;
using Keep.Controls;
using Keep.ViewModels;
using Keep.Models;

namespace Keep
{
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel viewModel = new MainPageViewModel();

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public MainPage()
        {
            this.InitializeComponent();
            
            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.DataContext = viewModel;

#if WINDOWS_PHONE_APP
            NotesListView.ReorderMode = ListViewReorderMode.Disabled;
#endif
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

        
        private void NoteContainer_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void NotesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if ((sender as ListView).ReorderMode == ListViewReorderMode.Enabled)
                return;
#endif

            object parameter = e.ClickedItem;
            Frame.Navigate(typeof(NoteEditPage), parameter);
        }

        private void NewTextNoteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            object parameter = new Note();
            Frame.Navigate(typeof(NoteEditPage), parameter);
        }

        private void NewChecklistNoteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            object parameter = new Note(String.Empty, new Checklist());
            Frame.Navigate(typeof(NoteEditPage), parameter);
        }

//        private void ReorderAppBarButton_Click(object sender, RoutedEventArgs e)
//        {
//#if WINDOWS_PHONE_APP
//            NotesListView.ReorderMode = ListViewReorderMode.Enabled;
//#endif
//        }

        private void ReorderAppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            NotesListView.ReorderMode = ListViewReorderMode.Enabled;
#endif
        }

        private void ReorderAppBarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            NotesListView.ReorderMode = ListViewReorderMode.Disabled;
#endif
        }

        public void OnReorderModeEnabled()
        {
            ReorderAppBarToggleButton.IsChecked = true;
        }

        public void OnReorderModeDisabled()
        {
            ReorderAppBarToggleButton.IsChecked = false;
        }

        private void NoteDeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if(!(((FrameworkElement)sender).DataContext is Note))
                return;

            Note note = ((FrameworkElement)sender).DataContext as Note;

            if(viewModel.DeleteNoteCommand.CanExecute(note)) 
                viewModel.DeleteNoteCommand.Execute(note);
        }
    }
}
