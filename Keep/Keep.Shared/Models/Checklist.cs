using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Keep.Utils;
using System.Diagnostics;

namespace Keep.Models
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
