using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;
using System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Keep.Views
{
    public sealed partial class NoteEditPage : Page
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public NoteEditViewModel viewModel { get { return (NoteEditViewModel)DataContext; } }
        private Brush previousBackground;

        public NoteEditPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            //Color Picker
            ColorPickerAppBarToggleButton.Checked += (s, _e) => NoteColorPicker.Open();
            ColorPickerAppBarToggleButton.Unchecked += (s, _e) => NoteColorPicker.Close();
            NoteColorPicker.Opened += (s, _e) => { ColorPickerAppBarToggleButton.IsChecked = true; };
            NoteColorPicker.Closed += (s, _e) => { ColorPickerAppBarToggleButton.IsChecked = false; };
            NoteColorPicker.SelectionChanged += (s, _e) => { viewModel.Note.Color = _e.AddedItems[0] as NoteColor; };
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.ChangeStatusBarColor(Colors.Black);

            if(e.NavigationParameter is Note)
                viewModel.Note = e.NavigationParameter as Note;

            viewModel.Note.Changed = false;

            previousBackground = App.RootFrame.Background;
            App.RootFrame.Background = Background;

        }

        private async void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            App.RootFrame.Background = previousBackground;

            if (viewModel.Note.Changed)
                await AppData.CreateOrUpdateNote(viewModel.Note);
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

        private void NewChecklistItemTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            bool isChecked = NewChecklistItemCheckbox.IsChecked == true;
            string text = NewChecklistItemTextBox.Text;

            if (e.Key == Windows.System.VirtualKey.Enter && !String.IsNullOrEmpty(text))
            {
                viewModel.Note.Checklist.Add(new ChecklistItem(text, isChecked));

                NewChecklistItemCheckbox.IsChecked = false;
                NewChecklistItemTextBox.Text = String.Empty;
            }
        }
    }
}
