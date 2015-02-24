using Keep.Common;
using Keep.Converters;
using Keep.Models;
using Keep.Utils;
using Keep.Views;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;

namespace Keep.ViewModels
{
    public class NoteEditViewModel : ViewModelBase
    {
#if WINDOWS_PHONE_APP
        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;
#endif

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

                AlreadyExists = AppData.Notes.Where<Note>(x => x.ID == Note.ID).FirstOrDefault<Note>() != null;
                IsArchived = AppData.ArchivedNotes.Where<Note>(x => x.ID == Note.ID).FirstOrDefault<Note>() != null;
                IsNewNote = !AlreadyExists && !IsArchived;
                IsPinned = SecondaryTile.Exists(Note.ID);

                NotifyPropertyChanged("ArchivedAtFormatedString");
                NotifyPropertyChanged("UpdatedAtFormatedString");
                NotifyPropertyChanged("CreatedAtFormatedString");

                Note.PropertyChanged += (s, _e) =>
                {
                    Debug.WriteLine("PROPPPRPROR: " + _e.PropertyName);
                    switch (_e.PropertyName)
                    {
                        case "Changed":
                            if (AlreadyExists)
                                AppData.HasUnsavedChangesOnNotes = Note.Changed;
                            else if (IsArchived)
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

        private void OnNotesSaved(object sender, EventArgs e)
        {
            NotifyPropertyChanged("Note");
            if (Note == null) return;

            if (!IsArchived)
                Note.Changed = false;

            TileManager.UpdateNoteTileIfExists(Note);
        }

        private void OnArchivedNotesSaved(object sender, EventArgs e)
        {
            NotifyPropertyChanged("Note");
            if (Note == null) return;

            if (IsArchived)
                Note.Changed = false;
        }

        public static Note CurrentNoteBeingEdited { get; set; }
        private static FriendlyTimeConverter friendlyTimeConverter = new FriendlyTimeConverter();

        public Note Note { get { return note; } set { note = value == null ? new Note() : value; NotifyPropertyChanged("Note"); } }
        private static Note note = new Note();

        public bool IsPinned { get { isPinned = Note == null ? false : SecondaryTile.Exists(Note.ID); return isPinned; } set { isPinned = value; NotifyPropertyChanged("IsPinned"); } }
        private bool isPinned;

        public bool IsNewNote { get { return isNewNote; } set { isNewNote = value; NotifyPropertyChanged("IsNewNote"); } }
        private bool isNewNote;

        public bool IsArchived { get { return isArchived; } set { isArchived = value; NotifyPropertyChanged("IsArchived"); } }
        private bool isArchived;

        public bool AlreadyExists { get { return alreadyExists; } set { alreadyExists = value; NotifyPropertyChanged("AlreadyExists"); } }
        private bool alreadyExists;

        public NoteImage TempNoteImage { get { return tempNoteImage; } set { tempNoteImage = value; } }
        private static NoteImage tempNoteImage = null;

        public string ArchivedAtFormatedString { get { return string.Format(LocalizedResources.ArchivedAtFormatString, friendlyTimeConverter.Convert(Note.ArchivedAt)); } }
        public string UpdatedAtFormatedString { get { return string.Format(LocalizedResources.UpdatedAtFormatString, friendlyTimeConverter.Convert(Note.UpdatedAt)); } }
        public string CreatedAtFormatedString { get { return string.Format(LocalizedResources.CreatedAtFormatString, friendlyTimeConverter.Convert(Note.CreatedAt)); } }

#if WINDOWS_PHONE_APP
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
#endif
#region COMMANDS_ACTIONS

        private void OpenImagePicker()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "open_image_picker", 1);

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

        private void ToggleChecklist()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "toggle_checklist", 0);
            Note.ToggleChecklist();
        }

        private async void Pin()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "pin", 0);

            if (Note.IsEmpty()) return;

            if (IsNewNote)
                await AppData.CreateOrUpdateNote(Note);

            IsPinned = await TileManager.CreateOrUpdateNoteTile(Note);
        }

        private void Unpin()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "unpin", 0);

            TileManager.RemoveTileIfExists(Note);
            IsPinned = SecondaryTile.Exists(Note.ID);
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
            bool success = IsArchived ? await AppData.RemoveArchivedNote(Note) : await AppData.RemoveNote(Note);
            if (!success) return;

            note = null;

            if (App.RootFrame.CanGoBack)
                App.RootFrame.GoBack();
            else if (IsArchived)
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
            if (IsArchived)
                await AppData.SaveArchivedNotes();
            else
                await AppData.SaveNotes();
        }

#endregion
    }
}