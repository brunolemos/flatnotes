using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using Keep.Common;

namespace Keep.Models
{
    public class Checklist : TrulyObservableCollection<ChecklistItem>
    {
        public Checklist() : base(false) { }
    }

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