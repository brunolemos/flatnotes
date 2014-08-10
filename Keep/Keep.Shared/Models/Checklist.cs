using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Keep.Utils;

namespace Keep.Models
{
    public class Checklist : TrulyObservableCollection<ChecklistItem> { }

    [DataContract]
    public class ChecklistItem : BaseModel
    {
        [DataMember]
        public String Text { get { return text; } set { if (text != value) { text = value; NotifyPropertyChanged("Text"); } } }
        private String text;

        [DataMember]
        public bool IsChecked { get { return isChecked; } set { if (isChecked != value) { isChecked = value; NotifyPropertyChanged("IsChecked"); } } }
        private bool isChecked;

        public ChecklistItem(String text = "", bool isChecked = false)
        {
            this.Text = text;
            this.IsChecked = isChecked;
        }
    }
}
