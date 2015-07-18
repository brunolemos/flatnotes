using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.Views;
using System;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;

        public RelayCommand CreateTextNoteCommand { get; private set; }
        public RelayCommand CreateChecklistNoteCommand { get; private set; }
        public RelayCommand ToggleSingleColumnViewCommand { get; private set; }
        public RelayCommand OpenArchivedNotesCommand { get; private set; }
        public RelayCommand OpenSettingsCommand { get; private set; }

        public Notes Notes { get { return notes; } private set { notes = value; NotifyPropertyChanged("Notes"); } }
        private Notes notes = AppData.Notes;

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
        public int Columns { get { return IsSingleColumnEnabled ? 1 : AppSettings.Instance.Columns; } }
        private bool IsSingleColumnEnabled { get { return AppSettings.Instance.IsSingleColumnEnabled; } set { AppSettings.Instance.IsSingleColumnEnabled = value; NotifyPropertyChanged("IsSingleColumnEnabled"); NotifyPropertyChanged("Columns"); } }

        public MainViewModel()
        {
            CreateTextNoteCommand = new RelayCommand(CreateTextNote);
            CreateChecklistNoteCommand = new RelayCommand(CreateChecklistNote);
            ToggleSingleColumnViewCommand = new RelayCommand(ToggleSingleColumnView, CanToggleSingleColumn);
            OpenArchivedNotesCommand = new RelayCommand(OpenArchivedNotes);
            OpenSettingsCommand = new RelayCommand(App.OpenSettings);

            AppData.NotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            AppSettings.Instance.ColumnsChanged += (s, e) =>
            {
                NotifyPropertyChanged("Columns");
                ToggleSingleColumnViewCommand.RaiseCanExecuteChanged();
            };
        }

#region COMMANDS_ACTIONS
        private void CreateTextNote()
        {
            App.TelemetryClient.TrackEvent("CreateNote_MainViewModel");
            App.RootFrame.Navigate(typeof(NoteEditPage), new Note());
        }

        private void CreateChecklistNote()
        {
            App.TelemetryClient.TrackEvent("CreateChecklistNote_MainViewModel");
            App.RootFrame.Navigate(typeof(NoteEditPage), new Note(true));
        }

        private void ToggleSingleColumnView()
        {
            App.TelemetryClient.TrackEvent("ToggleSingleColumnView_MainViewModel");
            IsSingleColumnEnabled = !IsSingleColumnEnabled;
        }

        private bool CanToggleSingleColumn()
        {
            return AppSettings.Instance.Columns != 1;
        }

        private void OpenArchivedNotes()
        {
            App.RootFrame.Navigate(typeof(ArchivedNotesPage));
        }
#endregion
    }
}