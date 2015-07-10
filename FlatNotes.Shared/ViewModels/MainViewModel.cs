using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;

        public RelayCommand CreateTextNoteCommand { get; private set; }
        public RelayCommand CreateChecklistNoteCommand { get; private set; }
        public RelayCommand OpenImagePickerCommand { get; private set; }
        public RelayCommand OpenArchivedNotesCommand { get; private set; }
        public RelayCommand OpenSettingsCommand { get; private set; }

        public Notes Notes { get { return notes; } private set { notes = value; NotifyPropertyChanged("Notes"); } }
        private Notes notes = AppData.Notes;

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

        private async void OpenImagePicker()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "open_image_picker", 0);

            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            //image
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

#if WINDOWS_PHONE_APP
            //open
            picker.PickSingleFileAndContinue();
#elif WINDOWS_UAP
            var file = await picker.PickSingleFileAsync();
            handleImageFromFilePicker(file);

#endif
        }

        private void handleImageFromFilePicker(StorageFile file)
        {
            handleImageFromFilePicker(new List<StorageFile>() { file });
        }

        private async void handleImageFromFilePicker(IReadOnlyList<StorageFile> files)
        {
            if (files == null || files.Count <= 0) return;
            string error = "";

            Note note = new Note();

            try
            {
                //add new images
                foreach (var file in files)
                {
                    Debug.WriteLine("Picked photo: " + file.Path);

                    NoteImage noteImage = new NoteImage();

                    StorageFile savedImage = await AppSettings.Instance.SaveImage(file, note.ID, noteImage.ID);

                    var imageProperties = await savedImage.Properties.GetImagePropertiesAsync();
                    noteImage.URL = savedImage.Path;
                    noteImage.Size = new Size(imageProperties.Width, imageProperties.Height);

                    note.Images.Add(noteImage);
                    break;
                }
            }
            catch (Exception e) { error = e.Message; }

            if (!String.IsNullOrEmpty(error))
            {
                GoogleAnalytics.EasyTracker.GetTracker().SendException(String.Format("Failed to save image ({0})", error), false);
                return;
            }

            //save and open edit page
            await AppData.CreateOrUpdateNote(note);
            App.RootFrame.Navigate(typeof(NoteEditPage), note);
        }

        private void OpenArchivedNotes()
        {
            App.RootFrame.Navigate(typeof(ArchivedNotesPage));
        }

#endregion
    }
}