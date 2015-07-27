using FlatNotes.Models;
using FlatNotes.Utils;
using System.Collections.Generic;

namespace FlatNotes.ViewModels
{
    public class ArchivedNotesViewModel : ViewModelBase
    {
        public static ArchivedNotesViewModel Instance { get { if (instance == null) instance = new ArchivedNotesViewModel(); return instance; } }
        private static ArchivedNotesViewModel instance = null;

        public Notes Notes { get { System.Diagnostics.Debug.WriteLine("AAA " + notes?.Count); return notes; } set { notes = value; NotifyPropertyChanged("Notes"); } }
        public Notes notes = AppData.ArchivedNotes;

        public int Columns { get { return -1; } }// AppSettings.Instance.Columns; } internal set { AppSettings.Instance.Columns = value; } }


        private ArchivedNotesViewModel()
        {
            AppData.ArchivedNotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            //AppSettings.Instance.ColumnsChanged += (s, e) => NotifyPropertyChanged("Columns");
        }
    }
}