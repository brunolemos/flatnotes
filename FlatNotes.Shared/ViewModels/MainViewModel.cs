using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using System;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public static MainViewModel Instance { get { if (instance == null) instance = new MainViewModel(); return instance; } }
        private static MainViewModel instance = null;

        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;

        public RelayCommand CreateTextNoteCommand { get; private set; }
        public RelayCommand CreateChecklistNoteCommand { get; private set; }
        public RelayCommand CreateImageNoteCommand { get; private set; }
        public RelayCommand ToggleSingleColumnViewCommand { get; private set; }
        public RelayCommand OpenNotesCommand { get; private set; }
        public RelayCommand OpenArchivedNotesCommand { get; private set; }
        public RelayCommand OpenSettingsCommand { get; private set; }

        public Action<object> ShowNote { get { return showNote; } set { showNote = value; NotifyPropertyChanged("ShowNote"); } }
        private Action<object> showNote;

        public Notes Notes { get { return notes; } set { if (notes == value) return; notes = value; NotifyPropertyChanged("Notes"); } }
        private Notes notes;

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
        public int Columns { get { return IsSingleColumnEnabled ? 1 : -1; } }
        private bool IsSingleColumnEnabled { get { return AppSettings.Instance.IsSingleColumnEnabled; } set { AppSettings.Instance.IsSingleColumnEnabled = value; NotifyPropertyChanged("IsSingleColumnEnabled"); } }

        private MainViewModel()
        {
            CreateTextNoteCommand = new RelayCommand(CreateTextNote);
            CreateChecklistNoteCommand = new RelayCommand(CreateChecklistNote);
            CreateImageNoteCommand = new RelayCommand(CreateImageNote);
            ToggleSingleColumnViewCommand = new RelayCommand(ToggleSingleColumnView);
            OpenNotesCommand = new RelayCommand(OpenNotesPage);
            OpenArchivedNotesCommand = new RelayCommand(OpenArchivedNotesPage);
            OpenSettingsCommand = new RelayCommand(OpenSettingsPage);

            AppData.NotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            AppSettings.Instance.IsSingleColumnEnabledChanged += (s, e) => { NotifyPropertyChanged("IsSingleColumnEnabled"); NotifyPropertyChanged("Columns"); };
        }

        #region COMMANDS_ACTIONS
        public void CreateTextNote()
        {
            App.TelemetryClient.TrackEvent("CreateNote_MainViewModel");
            ShowNote(new Note());
        }

        private void CreateChecklistNote()
        {
            App.TelemetryClient.TrackEvent("CreateChecklistNote_MainViewModel");
            ShowNote(new Note(true));
        }

        private void CreateImageNote()
        {
            App.TelemetryClient.TrackEvent("CreateImageNote_MainViewModel");
            ShowNote(new NoteImage());
        }

        private void ToggleSingleColumnView()
        {
            App.TelemetryClient.TrackEvent("ToggleSingleColumnView_MainViewModel");
            IsSingleColumnEnabled = !IsSingleColumnEnabled;
        }

        #endregion
    }
}