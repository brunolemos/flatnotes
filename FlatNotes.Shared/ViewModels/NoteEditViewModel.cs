using FlatNotes.Common;
using FlatNotes.Converters;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.ViewModels
{
    public class NoteEditViewModel : ViewModelBase
    {
        public static NoteEditViewModel Instance { get { if (instance == null) instance = new NoteEditViewModel(); return instance; } }
        private static NoteEditViewModel instance = null;

        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;

        public RelayCommand OpenImagePickerCommand { get; private set; }
        public RelayCommand ToggleChecklistCommand { get; private set; }
        public RelayCommand PinCommand { get; private set; }
        public RelayCommand UnpinCommand { get; private set; }
        public RelayCommand ArchiveNoteCommand { get; private set; }
        public RelayCommand RestoreNoteCommand { get; private set; }
        public RelayCommand DeleteNoteCommand { get; private set; }
        public RelayCommand DeleteNoteImageCommand { get; private set; }

        private NoteEditViewModel()
        {
            OpenImagePickerCommand = new RelayCommand(OpenImagePicker);
            ToggleChecklistCommand = new RelayCommand(ToggleChecklist);
            PinCommand = new RelayCommand(Pin);
            UnpinCommand = new RelayCommand(Unpin);
            ArchiveNoteCommand = new RelayCommand(ArchiveNote);
            RestoreNoteCommand = new RelayCommand(RestoreNote);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
            DeleteNoteImageCommand = new RelayCommand(DeleteNoteImage);

            PropertyChanged += OnPropertyChanged;

            AppSettings.Instance.NotesSaved += OnNotesSaved;
            AppSettings.Instance.ArchivedNotesSaved += OnArchivedNotesSaved;
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Note")
            {
                CurrentNoteBeingEdited = Note;
                if (Note == null) return;

                IsNewNote = AppData.DB.Find<Note>(Note.ID) != null;
                Note.IsPinned = SecondaryTile.Exists(Note.ID);

                NotifyPropertyChanged("ArchivedAtFormatedString");
                NotifyPropertyChanged("UpdatedAtFormatedString");
                NotifyPropertyChanged("CreatedAtFormatedString");
            }
        }

        private async void OnNotesSaved(object sender, EventArgs e)
        {
            NotifyPropertyChanged("Note");
            if (Note == null) return;

            if (!Note.IsArchived)
                Note.Changed = false;

            await TileManager.UpdateNoteTileIfExists(Note, AppSettings.Instance.TransparentNoteTile);
        }

        private void OnArchivedNotesSaved(object sender, EventArgs e)
        {
            NotifyPropertyChanged("Note");
            if (Note == null) return;

            if (Note.IsArchived)
                Note.Changed = false;
        }

        public static Note CurrentNoteBeingEdited { get; set; }
        private static FriendlyTimeConverter friendlyTimeConverter = new FriendlyTimeConverter();

        public Note Note {
            get {
                if (note == null) Note = new Note();
                return note;
            }
            set {
                if(note != null) note.PropertyChanged -= OnNotePropertyChanged;
                note = value == null ? new Note() : value;
                note.PropertyChanged += OnNotePropertyChanged;

                NotifyPropertyChanged("Note");
            }
        }
        private static Note note;

        public bool IsNewNote { get { return isNewNote; } set { isNewNote = value; NotifyPropertyChanged("IsNewNote"); } }
        private bool isNewNote;

        public NoteImage TempNoteImage { get { return tempNoteImage; } set { tempNoteImage = value; } }
        private static NoteImage tempNoteImage = null;

        public string ArchivedAtFormatedString { get { return string.Format(LocalizedResources.ArchivedAtFormatString, friendlyTimeConverter.Convert(Note.ArchivedAt)); } }
        public string UpdatedAtFormatedString { get { return string.Format(LocalizedResources.UpdatedAtFormatString, friendlyTimeConverter.Convert(Note.UpdatedAt)); } }
        public string CreatedAtFormatedString { get { return string.Format(LocalizedResources.CreatedAtFormatString, friendlyTimeConverter.Convert(Note.CreatedAt)); } }

        public ListViewReorderMode ReorderMode
        {
            get { return reorderMode; }
            set
            {
                if (reorderMode == value) return;

                reorderMode = value;
                NotifyPropertyChanged("ReorderMode");

                var handler = value == ListViewReorderMode.Enabled ? ReorderModeEnabled : ReorderModeDisabled;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }
        public ListViewReorderMode reorderMode = ListViewReorderMode.Disabled;
        #region COMMANDS_ACTIONS

        private async void OpenImagePicker()
        {
            App.TelemetryClient.TrackEvent("OpenImagePicker_NoteEditViewModel");

            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            //image
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

#if WINDOWS_PHONE_APP
            await Task.Delay(0);
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
            bool error = false;

            try
            {
                //delete old images
                await AppData.RemoveNoteImages(Note.Images);

                //clear image list
                Note.Images.Clear();

                //add new images
                foreach (var file in files)
                {
                    Debug.WriteLine("Picked photo: " + file.Path);

                    NoteImage noteImage = new NoteImage();

                    StorageFile savedImage = await AppSettings.Instance.SaveImage(file, Note.ID, noteImage.ID);

                    var imageProperties = await savedImage.Properties.GetImagePropertiesAsync();
                    noteImage.NoteId = Note.ID;
                    noteImage.URL = savedImage.Path;
                    noteImage.Width = imageProperties.Width;
                    noteImage.Height = imageProperties.Height;

                    Note.Images.Add(noteImage);
                    break;
                }

                Note.NotifyPropertyChanged("Images");
            }
            catch (Exception e)
            {
                error = true;

                var exceptionProperties = new Dictionary<string, string>() { { "Details", "Failed to save images" } };

                var exceptionMetrics = new Dictionary<string, double>();
                exceptionMetrics.Add("Image count", Note.Images.Count);
                for (int i = 0; i < Note.Images.Count; i++)
                {
                    exceptionMetrics.Add(string.Format("Image[{0}] Width", i), Note.Images[i].Width);
                    exceptionMetrics.Add(string.Format("Image[{0}] Height", i), Note.Images[i].Height);
                }

                App.TelemetryClient.TrackException(e, exceptionProperties, exceptionMetrics);
            }

            if (error)
            {
                //await(new MessageDialog("Failed to save image. Try again.", "Sorry")).ShowAsync();
                return;
            }

            //save
            await AppData.CreateOrUpdateNote(Note);
        }

        private void ToggleChecklist()
        {
            App.TelemetryClient.TrackEvent("ToggleChecklist_NoteEditViewModel");
            Note.ToggleChecklist();
        }

        private async void Pin()
        {
            App.TelemetryClient.TrackEvent("Pin_EditViewModel");

            if (Note.IsEmpty()) return;

            if (IsNewNote)
                await AppData.CreateOrUpdateNote(Note);

            Note.IsPinned = await TileManager.CreateOrUpdateNoteTile(Note, AppSettings.Instance.TransparentNoteTile);
        }

        private void Unpin()
        {
            App.TelemetryClient.TrackEvent("Unpin_NoteEditViewModel");

            TileManager.RemoveTileIfExists(Note.ID);
            Note.IsPinned = false;// SecondaryTile.Exists(Note.ID);
        }

        private void ArchiveNote()
        {
            App.TelemetryClient.TrackEvent("Archive_NoteEditViewModel");
            AppData.ArchiveNote(Note);
            note = null;

            if (App.RootFrame.CanGoBack)
                App.RootFrame.GoBack();
            else
                App.RootFrame.Navigate(typeof(MainPage));
        }

        private void RestoreNote()
        {
            App.TelemetryClient.TrackEvent("Restore_NoteEditViewModel");
            AppData.RestoreNote(Note);
            note = null;

            if (App.RootFrame.CanGoBack)
                App.RootFrame.GoBack();
            else
                App.RootFrame.Navigate(typeof(ArchivedNotesPage));
        }

        private async void DeleteNote()
        {
            App.TelemetryClient.TrackEvent("Delete_NoteEditViewModel");
            bool success = await AppData.RemoveNote(Note);
            if (!success) return;

            note = null;

            if (App.RootFrame.CanGoBack)
                App.RootFrame.GoBack();
            else if (Note.IsArchived)
                App.RootFrame.Navigate(typeof(ArchivedNotesPage));
            else
                App.RootFrame.Navigate(typeof(MainPage));
        }

        public async void DeleteNoteImage()
        {
            App.TelemetryClient.TrackEvent("DeleteNoteImage_NoteEditViewModel");
            if (TempNoteImage == null) return;

            bool success = await AppData.RemoveNoteImage(TempNoteImage);
            if (!success) return;

            Note.Images.Remove(TempNoteImage);
            TempNoteImage = null;
        }

        #endregion

        void OnNotePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ArchivedAt":
                    NotifyPropertyChanged("ArchivedAtFormatedString");
                    break;

                case "UpdatedAt":
                    NotifyPropertyChanged("UpdatedAtFormatedString");
                    break;

                case "CreatedAt":
                    NotifyPropertyChanged("CreatedAtFormatedString");
                    break;
            }

            if (!(e.PropertyName == "IsChecklist"
                || e.PropertyName == "Title" || e.PropertyName == "Text"
                || e.PropertyName == "Checklist" || e.PropertyName == "Images"
                || e.PropertyName == "Color" || e.PropertyName == "UpdatedAt"))
                return;

            Debug.WriteLine("Note_PropertyChanged " + e.PropertyName);
            Note.Changed = true;

            if (e.PropertyName == "UpdatedAt") return;
            Note.Touch();
        }
    }
}