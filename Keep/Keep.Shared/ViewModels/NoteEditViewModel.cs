using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.Views;

namespace Keep.ViewModels
{
    public class NoteEditViewModel : ViewModelBase
    {
        public RelayCommand ToggleChecklistCommand { get; private set; }
        public RelayCommand DeleteNoteCommand { get; private set; }

        public NoteEditViewModel()
        {
            ToggleChecklistCommand = new RelayCommand(ToggleChecklist);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
        }

        public Note Note { get { return note; } set { note = value; NotifyPropertyChanged("Note"); } }
        private Note note = new Note();

        #region COMMANDS_ACTIONS

        private void ToggleChecklist()
        {
            Note.ToggleChecklist();
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