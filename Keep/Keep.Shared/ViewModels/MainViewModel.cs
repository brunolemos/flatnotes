using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.Views;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace Keep.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;

        public RelayCommand CreateTextNoteCommand { get; private set; }
        public RelayCommand CreateChecklistNoteCommand { get; private set; }
        public RelayCommand OpenImagePickerCommand { get; private set; }
        public RelayCommand OpenArchivedNotesCommand { get; private set; }
        public RelayCommand SendFeedbackCommand { get; private set; }
        public RelayCommand SuggestFeatureOrReportBugCommand { get; private set; }
        public RelayCommand OpenSettingsCommand { get; private set; }

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
            OpenImagePickerCommand = new RelayCommand(OpenImagePicker);
            OpenArchivedNotesCommand = new RelayCommand(OpenArchivedNotes);
            SendFeedbackCommand = new RelayCommand(SendFeedback);
            SuggestFeatureOrReportBugCommand = new RelayCommand(SuggestFeatureOrReportBug);
            OpenSettingsCommand = new RelayCommand(OpenSettings);

            AppData.NotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            AppSettings.Instance.ColumnsChanged += (s, e) => NotifyPropertyChanged("Columns");
        }

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

            //open
            picker.PickSingleFileAndContinue();
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
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "send_feedback", 0);
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }


        private async void SuggestFeatureOrReportBug()
        {
            bool isBeta = Package.Current.Id.Name.Contains("Beta");
            string appName = isBeta ? "Keep Beta" : "Keep";

            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "suggest_feature_or_report_bug", isBeta ? 1 : 0);
            Windows.ApplicationModel.Email.EmailMessage mail = new Windows.ApplicationModel.Email.EmailMessage();

            mail.To.Add(new Windows.ApplicationModel.Email.EmailRecipient("keep@brunolemos.org"));
            mail.Subject = String.Format("Feedback - {0} v{1}.{2}.{3}.{4}", appName, Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
            mail.Body = ResourceLoader.GetForCurrentView().GetString("YourMessageGoesHere");

             await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(mail);
        }

        #endregion
    }
}