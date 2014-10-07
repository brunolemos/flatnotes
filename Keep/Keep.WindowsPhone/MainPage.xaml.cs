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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using Keep.Common;
using Keep.Commands;
using Keep.Controls;
using Keep.ViewModels;
using Keep.Models;
using Keep.Utils;

namespace Keep
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel viewModel = new MainViewModel();
        public static ChecklistMaxItemsConverter checklistMaxItemsConverter = new ChecklistMaxItemsConverter();
        Color? statusBarForegroundColor = null;

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        Note noteToDelete = null;

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
#if WINDOWS_PHONE_APP
            NotesListView.ReorderMode = ListViewReorderMode.Disabled;
#endif

            this.DataContext = viewModel;

            if (viewModel.Notes.Count > 0)
                VisualStateManager.GoToState(this, HasNotesVisualState.Name, false);
            else
                VisualStateManager.GoToState(this, EmptyNoteVisualState.Name, false);

            if (!String.IsNullOrEmpty(e.NavigationParameter.ToString()))
                Debug.WriteLine("MainPage NavigationParameter: " + e.NavigationParameter.ToString());

#if WINDOWS_PHONE_APP
            StatusBar statusBar = StatusBar.GetForCurrentView();
            statusBarForegroundColor = statusBar.ForegroundColor;

            if (AppSettings.Instance.LoggedUser.Preferences.Theme == ElementTheme.Light)
                statusBar.ForegroundColor = Color.FromArgb(0xFF, 0x5E, 0x5E, 0x5E);
            else
                statusBar.ForegroundColor = Color.FromArgb(0xFF, 0xCB, 0xCE, 0xD0);
#endif
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
#if WINDOWS_PHONE_APP
            StatusBar statusBar = StatusBar.GetForCurrentView();
            statusBar.ForegroundColor = statusBarForegroundColor;
#endif
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

        private void NoteChecklistListView_Loaded(object sender, RoutedEventArgs e)
        {
            int maxItems = 5;
            (sender as FrameworkElement).SetBinding(ListBox.ItemsSourceProperty, new Binding() { Path = new PropertyPath(""), Converter = checklistMaxItemsConverter, ConverterParameter = maxItems, Mode = BindingMode.OneWay });
        }

        private void NoteContainer_Holding(object sender, HoldingRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if (NotesListView.ReorderMode != ListViewReorderMode.Enabled)
                NotesListView.ReorderMode = ListViewReorderMode.Enabled;
#endif
            //FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
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

//        private void ReorderAppBarToggleButton_Checked(object sender, RoutedEventArgs e)
//        {
//#if WINDOWS_PHONE_APP
//            NotesListView.ReorderMode = ListViewReorderMode.Enabled;
//#endif
//        }

//        private void ReorderAppBarToggleButton_Unchecked(object sender, RoutedEventArgs e)
//        {
//#if WINDOWS_PHONE_APP
//            NotesListView.ReorderMode = ListViewReorderMode.Disabled;
//#endif
//        }

        //public void OnReorderModeEnabled()
        //{
        //    ReorderAppBarToggleButton.IsChecked = true;
        //}

        //public void OnReorderModeDisabled()
        //{
        //    ReorderAppBarToggleButton.IsChecked = false;
        //}

        //private void NoteDeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        //{
        //    if(!(((FrameworkElement)sender).DataContext is Note))
        //        return;

        //    Note note = ((FrameworkElement)sender).DataContext as Note;

        //    if(viewModel.DeleteNoteCommand.CanExecute(note)) 
        //        viewModel.DeleteNoteCommand.Execute(note);
        //}

        private void NoteContainer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            noteToDelete = (sender as FrameworkElement).DataContext as Note;
        }

        private void DeleteDropArea_DragEnter(object sender, DragEventArgs e)
        {

            (sender as Grid).Background = new SolidColorBrush(Color.FromArgb(0xF2, 0xC0, 0x39, 0x2B));
        }

        private void DeleteDropArea_DragLeave(object sender, DragEventArgs e)
        {
            (sender as Grid).Background = (SolidColorBrush)App.Current.Resources["PhoneChromeBrush"];
        }

        private void DeleteDropArea_Drop(object sender, DragEventArgs e)
        {
            (sender as Grid).Background = (SolidColorBrush)App.Current.Resources["PhoneChromeBrush"];

            if (viewModel.DeleteNoteCommand.CanExecute(noteToDelete))
                viewModel.DeleteNoteCommand.Execute(noteToDelete);
        }

        private void NotesListView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            Debug.WriteLine("NotesListView_DataContextChanged");
            if (!(args.NewValue is MainViewModel))
                return;

            if ((args.NewValue as MainViewModel).Notes.Count <= 0)
                VisualStateManager.GoToState(this, "EmptyNoteVisualState", true);
            else
                VisualStateManager.GoToState(this, "HasNotesVisualState", true);
        }

        private void SettingsAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}
