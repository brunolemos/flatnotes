using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Keep.Models
{
    public class Checklist : ObservableCollection<ChecklistItem> {}

    [DataContract]
    public class ChecklistItem : ModelBase
    {
        public string Text { get { return text; } set { text = value; NotifyPropertyChanged("Text"); } }
        [DataMember(Name = "Text")]
        public string text;

        public bool IsChecked { get { return isChecked; } set { isChecked = value; NotifyPropertyChanged("IsChecked"); } }
        [DataMember(Name = "IsChecked")]
        private bool isChecked;

        public ChecklistItem() { }

        public ChecklistItem(string text, bool isChecked = false)
        {
            this.text = text;
            this.isChecked = isChecked;
        }
    }
}