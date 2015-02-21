using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.Views;
using System;
using Windows.UI.Xaml.Controls;

namespace Keep.ViewModels
{
    public class ArchivedNotesViewModel : ViewModelBase
    {
#if WINDOWS_PHONE_APP
        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;
#endif

        public Notes Notes { get { return notes; } private set { notes = value; NotifyPropertyChanged("Notes"); } }
        public Notes notes = AppData.ArchivedNotes;

#if WINDOWS_PHONE_APP
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
#endif

        public bool ReorderedNotes { get; set; }

        public int Columns { get { return AppSettings.Instance.Columns; } internal set { AppSettings.Instance.Columns = value; } }

#region COMMANDS_ACTIONS

        public ArchivedNotesViewModel()
        {
            AppData.NotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            AppSettings.Instance.ColumnsChanged += (s, e) => NotifyPropertyChanged("Columns");
        }

#endregion
    }
}