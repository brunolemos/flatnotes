using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;

namespace FlatNotes.ViewModels
{
    public class ArchivedNotesViewModel : ViewModelBase
    {
        public static ArchivedNotesViewModel Instance { get { if (instance == null) instance = new ArchivedNotesViewModel(); return instance; } }
        private static ArchivedNotesViewModel instance = null;

        public RelayCommand ToggleSingleColumnViewCommand { get; private set; }

        public Notes Notes { get { return notes; } set { notes = value; NotifyPropertyChanged("Notes"); } }
        public Notes notes = AppData.ArchivedNotes;

        public int Columns { get { return IsSingleColumnEnabled ? 1 : -1; } }
        private bool IsSingleColumnEnabled { get { return AppSettings.Instance.IsSingleColumnEnabled; } set { AppSettings.Instance.IsSingleColumnEnabled = value; NotifyPropertyChanged("IsSingleColumnEnabled"); } }


        private ArchivedNotesViewModel()
        {
            ToggleSingleColumnViewCommand = new RelayCommand(ToggleSingleColumnView);

            AppData.ArchivedNotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            AppSettings.Instance.IsSingleColumnEnabledChanged += (s, e) => { NotifyPropertyChanged("IsSingleColumnEnabled"); NotifyPropertyChanged("Columns"); };
        }

        #region COMMANDS_ACTIONS

        private void ToggleSingleColumnView()
        {
            App.TelemetryClient.TrackEvent("ToggleSingleColumnView_ArchivedNotesViewModel");
            IsSingleColumnEnabled = !IsSingleColumnEnabled;
        }

        #endregion
    }
}