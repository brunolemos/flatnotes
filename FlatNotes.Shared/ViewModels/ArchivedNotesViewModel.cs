using FlatNotes.Models;
using FlatNotes.Utils;
using System;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.ViewModels
{
    public class ArchivedNotesViewModel : ViewModelBase
    {
        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;

        public Notes Notes { get { return notes; } private set { notes = value; NotifyPropertyChanged("Notes"); } }
        public Notes notes = AppData.ArchivedNotes;

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

        public int Columns { get { return -1; } }// AppSettings.Instance.Columns; } internal set { AppSettings.Instance.Columns = value; } }

#region COMMANDS_ACTIONS

        public ArchivedNotesViewModel()
        {
            AppData.NotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            AppSettings.Instance.ColumnsChanged += (s, e) => NotifyPropertyChanged("Columns");
        }

#endregion
    }
}