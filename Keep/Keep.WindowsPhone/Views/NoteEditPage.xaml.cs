using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Keep.Views
{
    public sealed partial class NoteEditPage : Page
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public NoteEditViewModel viewModel { get { return (NoteEditViewModel)DataContext; } }

        public NoteEditPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            //Watch
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (s, e) => App.Watch.Restart();
            this.Loaded += NoteEditPage_Loaded;

            //Color Picker
            ColorPickerAppBarToggleButton.Checked += (s, _e) => NoteColorPicker.Open();
            ColorPickerAppBarToggleButton.Unchecked += (s, _e) => NoteColorPicker.Close();
            NoteColorPicker.Opened += (s, _e) => { ColorPickerAppBarToggleButton.IsChecked = true; };
            NoteColorPicker.Closed += (s, _e) => { ColorPickerAppBarToggleButton.IsChecked = false; };
            NoteColorPicker.SelectionChanged += (s, _e) => { viewModel.Note.Color = _e.AddedItems[0] as NoteColor; };
        }

        private void NoteEditPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            App.Watch.Stop();
            LoadedEllapsedTime.Text = String.Format("content shown after {0}ms", App.Watch.ElapsedMilliseconds.ToString());
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            LoadStateEllapsedTime.Text = String.Format("navigated in {0}ms", App.Watch.ElapsedMilliseconds.ToString());

            if(e.NavigationParameter is Note)
                viewModel.Note = e.NavigationParameter as Note;

            viewModel.Note.Changed = false;
        }

        private async void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (viewModel.Note.Changed)
                await AppData.CreateOrUpdateNote(viewModel.Note);

            viewModel.Note = null;
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
