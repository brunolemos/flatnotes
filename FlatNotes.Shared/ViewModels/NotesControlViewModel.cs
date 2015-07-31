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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.ViewModels
{
    public class NotesControlViewModel : ViewModelBase
    {
        public static NotesControlViewModel Instance { get { if (instance == null) instance = new NotesControlViewModel(); return instance; } }
        private static NotesControlViewModel instance = null;

        public RelayCommand<Note> ArchiveNoteCommand { get; private set; }
        public RelayCommand<Note> RestoreNoteCommand { get; private set; }
        public RelayCommand<Note> DeleteNoteCommand { get; private set; }
        public RelayCommand<Note> PinCommand { get; private set; }
        public RelayCommand<Note> UnpinCommand { get; private set; }

        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;

        private NotesControlViewModel()
        {
            ArchiveNoteCommand = new RelayCommand<Note>(ArchiveNote);
            RestoreNoteCommand = new RelayCommand<Note>(RestoreNote);
            DeleteNoteCommand = new RelayCommand<Note>(DeleteNote);
            PinCommand = new RelayCommand<Note>(Pin);
            UnpinCommand = new RelayCommand<Note>(Unpin);
        }

        public NoteColors Colors { get { return NoteColor.Colors; } }

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
        public bool ReorderedNotes { get; set; }

        #region COMMANDS_ACTIONS

        private void ArchiveNote(Note note)
        {
            if (note == null) return;
            App.TelemetryClient.TrackEvent("Archive_NotesControlViewModel");

            AppData.ArchiveNote(note);
        }

        private void RestoreNote(Note note)
        {
            if (note == null) return;
            App.TelemetryClient.TrackEvent("Restore_NotesControlViewModel");

            AppData.RestoreNote(note);
        }

        private async void DeleteNote(Note note)
        {
            if (note == null) return;
            App.TelemetryClient.TrackEvent("Delete_NotesControlViewModel");

            await AppData.RemoveNote(note);
        }

        private async void Pin(Note note)
        {
            if (note == null || note.IsEmpty()) return;
            App.TelemetryClient.TrackEvent("Pin_NotesControlViewModel");

            await TileManager.CreateOrUpdateNoteTile(note, AppSettings.Instance.TransparentNoteTile);
            note.NotifyPropertyChanged("IsPinned");
            note.NotifyPropertyChanged("CanPin");
        }

        private async void Unpin(Note note)
        {
            if (note == null) return;
            App.TelemetryClient.TrackEvent("Unpin_NoteNotesControlViewModel");

            TileManager.RemoveTileIfExists(note.ID);
            note.NotifyPropertyChanged("IsPinned");

            await Task.Delay(0500);
            note.NotifyPropertyChanged("IsPinned");
            note.NotifyPropertyChanged("CanPin");
        }

        public void ChangeColor(Note note, NoteColor newColor)
        {
            if (note == null || newColor == null) return;
            App.TelemetryClient.TrackEvent("ChangeColor_NoteNotesControlViewModel");

            AppData.ChangeNoteColor(note, newColor);
        }

        #endregion
    }
}