using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace FlatNotes.Utils.Migration.Versions.v1.Models
{
    public class Checklist : ObservableCollection<ChecklistItem> {}

    [DataContract]
    public class ChecklistItem : BaseModel
    {
        public String Text { get { return text; } set { if (text != value) { text = value; NotifyPropertyChanged("Text"); } } }
        [DataMember(Name = "Text")]
        private String text = String.Empty;

        public bool IsChecked { get { return isChecked; } set { if (isChecked != value) { isChecked = value; NotifyPropertyChanged("IsChecked"); } } }
        [DataMember(Name = "IsChecked")]
        private bool isChecked = false;

        public ChecklistItem(String text = "", bool isChecked = false) : base()
        {
            this.Text = text;
            this.IsChecked = isChecked;
        }
    }
}
