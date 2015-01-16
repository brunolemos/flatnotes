using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.Views;
using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

namespace Keep.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;

        public RelayCommand CreateTextNoteCommand { get; private set; }
        public RelayCommand CreateChecklistNoteCommand { get; private set; }
        public RelayCommand OpenArchivedNotesCommand { get; private set; }
        public RelayCommand SendFeedbackCommand { get; private set; }
        public RelayCommand OpenSettingsCommand { get; private set; }
        public RelayCommand DeleteNoteCommand { get; private set; }

        public Notes Notes { get { return notes; } private set { notes = value; NotifyPropertyChanged("Notes"); } }
        public Notes notes = AppData.Notes;

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

        #region COMMANDS_ACTIONS

        public MainViewModel()
        {
            CreateTextNoteCommand = new RelayCommand(CreateTextNote);
            CreateChecklistNoteCommand = new RelayCommand(CreateChecklistNote);
            OpenArchivedNotesCommand = new RelayCommand(OpenArchivedNotes);
            SendFeedbackCommand = new RelayCommand(SendFeedback);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            DeleteNoteCommand = new RelayCommand(DeleteNote, CanDeleteNote);

            AppData.NotesChanged += (s, e) => NotifyPropertyChanged("Notes");
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

        private void OpenArchivedNotes()
        {
            App.RootFrame.Navigate(typeof(ArchivedNotesPage));
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