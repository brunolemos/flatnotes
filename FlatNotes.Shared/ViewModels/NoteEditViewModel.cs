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

        public NoteEditViewModel()
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

                Note.AlreadyExists = AppData.Notes.Where<Note>(x => x.ID == Note.ID).FirstOrDefault<Note>() != null;
                Note.IsArchived = AppData.ArchivedNotes.Where<Note>(x => x.ID == Note.ID).FirstOrDefault<Note>() != null;
                Note.IsPinned = SecondaryTile.Exists(Note.ID);

                NotifyPropertyChanged("ArchivedAtFormatedString");
                NotifyPropertyChanged("UpdatedAtFormatedString");
                NotifyPropertyChanged("CreatedAtFormatedString");

                Note.PropertyChanged += (s, _e) =>
                {
                    //Debug.WriteLine("PROPPPRPROR: " + _e.PropertyName);

                    switch (_e.PropertyName)
                    {
                        case "Changed":
                            if (Note.AlreadyExists)
                                AppData.HasUnsavedChangesOnNotes = Note.Changed;
                            else if (Note.IsArchived)
                                AppData.HasUnsavedChangesOnArchivedNotes = Note.Changed;

                            break;

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
                };
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

        public Note Note { get { return note; } set { note = value == null ? new Note() : value; NotifyPropertyChanged("Note"); } }
        private static Note note = new Note();

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
                    noteImage.URL = savedImage.Path;
                    noteImage.Size = new Size(imageProperties.Width, imageProperties.Height);

                    Note.Images.Add(noteImage);
                    break;
                }
            }
            catch (Exception e)
            {
                error = true;

                var exceptionProperties = new Dictionary<string, string>() { { "Details", "Failed to save images" } };

                var exceptionMetrics = new Dictionary<string, double>();
                exceptionMetrics.Add("Image count", Note.Images.Count);
                for (int i = 0; i < Note.Images.Count; i++)
                {
                    exceptionMetrics.Add(string.Format("Image[{0}] Width", i), Note.Images[i].Size.Width);
                    exceptionMetrics.Add(string.Format("Image[{0}] Height", i), Note.Images[i].Size.Height);
                }

                App.TelemetryClient.TrackException(e, exceptionProperties, exceptionMetrics);
            }

            if (error)
            {
                //await(new MessageDialog("Failed to save image. Try again.", "Sorry")).ShowAsync();
                return;
            }

            //save
            for (int i = 0; i < 50; i++)
            {
                await AppData.CreateOrUpdateNote(Note);

            }
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

            if (Note.IsNewNote)
                await AppData.CreateOrUpdateNote(Note);

            Note.IsPinned = await TileManager.CreateOrUpdateNoteTile(Note, AppSettings.Instance.TransparentNoteTile);
        }

        private void Unpin()
        {
            App.TelemetryClient.TrackEvent("Unpin_NoteEditViewModel");

            TileManager.RemoveTileIfExists(Note);
            Note.IsPinned = false;// SecondaryTile.Exists(Note.ID);
        }

        private async void ArchiveNote()
        {
            await AppData.ArchiveNote(Note);
            note = null;

            if (App.RootFrame.CanGoBack)
                App.RootFrame.GoBack();
            else
                App.RootFrame.Navigate(typeof(MainPage));
        }

        private async void RestoreNote()
        {
            await AppData.RestoreNote(Note);
            note = null;

            if (App.RootFrame.CanGoBack)
                App.RootFrame.GoBack();
            else
                App.RootFrame.Navigate(typeof(ArchivedNotesPage));
        }

        private async void DeleteNote()
        {
            bool success = Note.IsArchived ? await AppData.RemoveArchivedNote(Note) : await AppData.RemoveNote(Note);
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
            if (TempNoteImage == null) return;

            bool success = await AppData.RemoveNoteImage(TempNoteImage);
            if (!success) return;

            Note.Images.Remove(TempNoteImage);
            TempNoteImage = null;

            //save
            if (Note.IsArchived)
                await AppData.SaveArchivedNotes();
            else
                await AppData.SaveNotes();
        }

#endregion
    }
}