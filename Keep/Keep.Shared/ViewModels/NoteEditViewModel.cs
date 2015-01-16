using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.Views;
using System;
using Windows.UI.Xaml.Controls;

namespace Keep.ViewModels
{
    public class NoteEditViewModel : ViewModelBase
    {
        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;

        public RelayCommand ToggleChecklistCommand { get; private set; }
        public RelayCommand ArchiveNoteCommand { get; private set; }
        public RelayCommand DeleteNoteCommand { get; private set; }

        public NoteEditViewModel()
        {
            ToggleChecklistCommand = new RelayCommand(ToggleChecklist);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
            ArchiveNoteCommand = new RelayCommand(ArchiveNote);
        }

        public Note Note { get { return note; } set { note = value; NotifyPropertyChanged("Note"); } }
        private Note note = new Note();

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

        private void ToggleChecklist()
        {
            Note.ToggleChecklist();
        }

        private async void ArchiveNote()
        {
            await AppData.ArchiveNote(Note);
            note = null;

            App.RootFrame.Navigate(typeof(MainPage));
        }

        private async void DeleteNote()
        {
            await AppData.RemoveNote(Note);
            note = null;

            App.RootFrame.Navigate(typeof(MainPage));
        }

        #endregion
    }
}