using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.Views;

namespace FlatNotes.ViewModels
{
    public class ArchivedNotesViewModel : ViewModelBase
    {
        public static ArchivedNotesViewModel Instance { get { if (instance == null) instance = new ArchivedNotesViewModel(); return instance; } }
        private static ArchivedNotesViewModel instance = null;

        public RelayCommand OpenMainPageCommand { get; private set; }
        public RelayCommand ToggleSingleColumnViewCommand { get; private set; }
        public RelayCommand OpenSettingsCommand { get; private set; }

        public Notes Notes { get { return notes; } set { notes = value; NotifyPropertyChanged("Notes"); } }
        public Notes notes = AppData.ArchivedNotes;

        public int Columns { get { return IsSingleColumnEnabled ? 1 : -1; } }
        private bool IsSingleColumnEnabled { get { return AppSettings.Instance.IsSingleColumnEnabled; } set { AppSettings.Instance.IsSingleColumnEnabled = value; NotifyPropertyChanged("IsSingleColumnEnabled"); } }


        private ArchivedNotesViewModel()
        {
            OpenMainPageCommand = new RelayCommand(OpenMainPage);
            ToggleSingleColumnViewCommand = new RelayCommand(ToggleSingleColumnView);
            OpenSettingsCommand = new RelayCommand(OpenSettingsPage);

            AppData.ArchivedNotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            AppSettings.Instance.IsSingleColumnEnabledChanged += (s, e) => { NotifyPropertyChanged("IsSingleColumnEnabled"); NotifyPropertyChanged("Columns"); };
        }

        #region COMMANDS_ACTIONS

        private void OpenMainPage()
        {
            App.RootFrame.Navigate(typeof(MainPage));
        }

        private void ToggleSingleColumnView()
        {
            App.TelemetryClient.TrackEvent("ToggleSingleColumnView_ArchivedNotesViewModel");
            IsSingleColumnEnabled = !IsSingleColumnEnabled;
        }

        private void OpenSettingsPage()
        {
            App.RootFrame.Navigate(typeof(SettingsPage));
        }

        #endregion
    }
}