using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.Views;
using System;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
#if WINDOWS_PHONE_APP
        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;
#endif

        public RelayCommand CreateTextNoteCommand { get; private set; }
        public RelayCommand CreateChecklistNoteCommand { get; private set; }
        public RelayCommand OpenImagePickerCommand { get; private set; }
        public RelayCommand OpenArchivedNotesCommand { get; private set; }
        public RelayCommand OpenSettingsCommand { get; private set; }

        public Notes Notes { get { return notes; } private set { notes = value; NotifyPropertyChanged("Notes"); } }
        private Notes notes = AppData.Notes;


#if WINDOWS_PHONE_APP
        public ListViewReorderMode ReorderMode {
            get { return reorderMode; }
            set {
                if (reorderMode == value) return;

                reorderMode = value;
                NotifyPropertyChanged("ReorderMode");

                var handler = value == ListViewReorderMode.Enabled ? ReorderModeEnabled : ReorderModeDisabled;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }
        public ListViewReorderMode reorderMode = ListViewReorderMode.Disabled;
#endif

        public bool ReorderedNotes { get; set; }

        public int Columns { get { return AppSettings.Instance.Columns; } internal set { AppSettings.Instance.Columns = value; } }

        public MainViewModel()
        {
            CreateTextNoteCommand = new RelayCommand(CreateTextNote);
            CreateChecklistNoteCommand = new RelayCommand(CreateChecklistNote);
            OpenImagePickerCommand = new RelayCommand(OpenImagePicker);
            OpenArchivedNotesCommand = new RelayCommand(OpenArchivedNotes);
            OpenSettingsCommand = new RelayCommand(App.OpenSettings);

            AppData.NotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            AppSettings.Instance.ColumnsChanged += (s, e) => NotifyPropertyChanged("Columns");
        }

#region COMMANDS_ACTIONS

        private void CreateTextNote()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "create_text_note", 0);
            App.RootFrame.Navigate(typeof(NoteEditPage), new Note());
        }

        private void CreateChecklistNote()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "create_checklist_note", 0);
            App.RootFrame.Navigate(typeof(NoteEditPage), new Note(true));
        }

        private void OpenImagePicker()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "open_image_picker", 0);

            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            //image
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

#if WINDOWS_PHONE_APP
            //open
            picker.PickSingleFileAndContinue();
#endif
        }

        private void OpenArchivedNotes()
        {
            App.RootFrame.Navigate(typeof(ArchivedNotesPage));
        }

#endregion
    }
}