using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.Views;
using System;
using Windows.ApplicationModel;

namespace Keep.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public RelayCommand CreateTextNoteCommand { get; private set; }
        public RelayCommand CreateChecklistNoteCommand { get; private set; }
        public RelayCommand SendFeedbackCommand { get; private set; }
        public RelayCommand OpenSettingsCommand { get; private set; }
        public RelayCommand DeleteNoteCommand { get; private set; }

        public Notes Notes { get { return notes; } private set { notes = value; } }
        public Notes notes = AppData.Notes;

        public int Columns { get { return AppSettings.Instance.Columns; } internal set { AppSettings.Instance.Columns = value; } }
        public bool ReorderedNotes { get; set; }

        #region COMMANDS_ACTIONS

        public MainViewModel()
        {
            CreateTextNoteCommand = new RelayCommand(CreateTextNote);
            CreateChecklistNoteCommand = new RelayCommand(CreateChecklistNote);
            SendFeedbackCommand = new RelayCommand(SendFeedback);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            DeleteNoteCommand = new RelayCommand(DeleteNote, CanDeleteNote);

            AppData.NotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            AppData.ArchivedNotesChanged += (s, e) => NotifyPropertyChanged("ArchivedNotes");
            AppSettings.Instance.ColumnsChanged += (s, e) => NotifyPropertyChanged("Columns");
        }

        private void CreateTextNote()
        {
            App.RootFrame.Navigate(typeof(NoteEditPage), new Note());
        }

        private void CreateChecklistNote()
        {
            App.RootFrame.Navigate(typeof(NoteEditPage), new Note(true));
        }

        private void OpenSettings()
        {
            App.RootFrame.Navigate(typeof(SettingsPage));
        }

        private async void SendFeedback()
        {
            Windows.ApplicationModel.Email.EmailMessage mail = new Windows.ApplicationModel.Email.EmailMessage();

            mail.To.Add(new Windows.ApplicationModel.Email.EmailRecipient("keep@brunolemos.org"));
            mail.Subject = String.Format("Feedback - Keep v{0}.{1}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor);
            mail.Body = "";

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(mail);
        }

        private bool CanDeleteNote()
        {
            return AppData.TempNote != null;
        }

        private async void DeleteNote()
        {
            await AppData.RemoveNote(AppData.TempNote);
        }

        #endregion
    }
}