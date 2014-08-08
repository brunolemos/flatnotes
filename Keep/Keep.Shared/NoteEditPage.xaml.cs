using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;
using System.Diagnostics;

namespace Keep
{
    public sealed partial class NoteEditPage : Page
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        NoteEditViewModel viewModel;
        //double lastStatusBarBackgroundOpacity;
        //Color? lastStatusBarBackgroundColor;
        Color? lastStatusBarForegroundColor;

        public NoteEditPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
#if WINDOWS_PHONE_APP
            StatusBar statusBar = StatusBar.GetForCurrentView();
            //lastStatusBarBackgroundOpacity = statusBar.BackgroundOpacity;
            //lastStatusBarBackgroundColor = statusBar.BackgroundColor;
            lastStatusBarForegroundColor = statusBar.ForegroundColor;

            if (Application.Current.RequestedTheme != ApplicationTheme.Light)
            {
                //statusBar.BackgroundColor = Colors.Black;
                //statusBar.BackgroundOpacity = 0.33;
                statusBar.ForegroundColor = Color.FromArgb(0xFF, 0x44, 0x44, 0x44);
            }

            //ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
#endif

            viewModel = new NoteEditViewModel();
            Note note = null;

            if (e.NavigationParameter != null && e.NavigationParameter is Note)
            {
                Notes notes = AppSettings.Instance.LoggedUser.Notes;
                note = notes.Where<Note>(x => x.ID == ((Note)e.NavigationParameter).ID).FirstOrDefault<Note>();
            }
            else if (e.NavigationParameter != null && e.NavigationParameter is string)
            {
                Notes notes = AppSettings.Instance.LoggedUser.Notes;
                note = notes.Where<Note>(x => x.ID == e.NavigationParameter.ToString()).FirstOrDefault<Note>();
            }

            viewModel.Note = (note != null ? note : new Note());
            this.DataContext = viewModel;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if (Application.Current.RequestedTheme != ApplicationTheme.Light)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                //statusBar.BackgroundOpacity = lastStatusBarBackgroundOpacity;
                //statusBar.BackgroundColor = lastStatusBarBackgroundColor;
                statusBar.ForegroundColor = lastStatusBarForegroundColor;
            }

            //ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
#endif

            if (viewModel.Note == null) return;
            
            //update binding (fix problem when creating a note and press back button while textbox is focused)
            ForceTextBoxBindingUpdate();

            Notes notes = AppSettings.Instance.LoggedUser.Notes;
            Note note = notes.Where<Note>(x => x.ID == viewModel.Note.ID).FirstOrDefault<Note>();

            if (note == null)
            {
                if (!(viewModel.Note == null || viewModel.Note.IsEmpty()))
                    AppSettings.Instance.LoggedUser.Notes.Insert(0, viewModel.Note);
            }
            else
            {
                if ((viewModel.Note == null || viewModel.Note.IsEmpty()))
                    if (viewModel.DeleteNoteCommand.CanExecute(viewModel.Note))
                        viewModel.DeleteNoteCommand.Execute(viewModel.Note);
            }
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



        public void ForceTextBoxBindingUpdate()
        {
            //force binding update (fix problem when editing textbox before click appbar)
            NoteTitleTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            NoteTextTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
        
        private void DeleteNoteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.DeleteNoteCommand.CanExecute(viewModel.Note))
            {
                viewModel.DeleteNoteCommand.Execute(viewModel.Note);
                viewModel.Note = null;

                navigationHelper.GoBack();
            }
        }

        private void ColorPickerAppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(this);
        }

        private void ColorPickerAppBarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            FlyoutBase.GetAttachedFlyout(this).Hide();
        }

        private void ColorPickerFlyout_Opened(object sender, object e)
        {
            ColorPickerAppBarToggleButton.IsChecked = true;
        }

        private void ColorPickerFlyout_Closed(object sender, object e)
        {
            ColorPickerAppBarToggleButton.IsChecked = false;
        }

        private void NoteColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 0)
                return;
            
            NoteColor newColor = e.AddedItems[0] as NoteColor;
            viewModel.Note.Color = newColor;
        }
    }
}
