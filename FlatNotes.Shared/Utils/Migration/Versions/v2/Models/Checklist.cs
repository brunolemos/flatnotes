using FlatNotes.Common;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FlatNotes.Utils.Migration.Versions.v2.Models
{
    public class Checklist : TrulyObservableCollection<ChecklistItem>
    {
        public Checklist() : base(false) { }

        public static implicit operator Checklist(List<ChecklistItem> _checklist)
        {
            var checklist = new Checklist();
            foreach (var item in _checklist)
                checklist.Add(item);

            return checklist;
        }

        public static implicit operator List<ChecklistItem>(Checklist _checklist)
        {
            var checklist = new List<ChecklistItem>();
            foreach (var item in _checklist)
                checklist.Add(item);

            return checklist;
        }

        public static implicit operator Checklist(FlatNotes.Utils.Migration.Versions.v1.Models.Checklist _checklist)
        {
            var checklist = new Checklist();
            foreach (var item in _checklist)
                checklist.Add(item);

            return checklist;
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v1.Models.Checklist(Checklist _checklist)
        {
            var checklist = new FlatNotes.Utils.Migration.Versions.v1.Models.Checklist();
            foreach (var item in _checklist)
                checklist.Add(item);

            return checklist;
        }
    }

    [DataContract]
    public class ChecklistItem : ModelBase
    {
        public const char CHECKED_SYMBOL = '☑';//▣
        public const char UNCHECKED_SYMBOL = '⬜';

        public string Text { get { return text; } set { text = value; NotifyPropertyChanged("Text"); } }
        [DataMember(Name = "Text")]
        public string text = "";

        public bool? IsChecked { get { return isChecked; } set { isChecked = value; NotifyPropertyChanged("IsChecked"); } }
        [DataMember(Name = "IsChecked")]
        private bool? isChecked = false;

        public ChecklistItem() { }

        public ChecklistItem(string text, bool isChecked = false)
        {
            this.text = text;
            this.isChecked = isChecked;
        }

        public static ChecklistItem FromText(string str)
        {
            bool isChecked = false;

            if (str[0] == CHECKED_SYMBOL || str[0] == UNCHECKED_SYMBOL)
            {
                isChecked = str[0] == '☑' ? true : false;
                str = str.Substring(2, str.Length - 2);
            }

            return new ChecklistItem(str, isChecked);
        }

        public override string ToString()
        {
            return Text;
        }

        public string ToString(bool showCheckedMark = false)
        {
            string checkMark = showCheckedMark ? (IsChecked == true ? CHECKED_SYMBOL + " " : UNCHECKED_SYMBOL + " ") : "";
            string str = checkMark + Text;

            return str.Trim();
        }

        public static implicit operator ChecklistItem(FlatNotes.Utils.Migration.Versions.v1.Models.ChecklistItem item)
        {
            return new ChecklistItem(item.Text, item.IsChecked == true);
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v1.Models.ChecklistItem(ChecklistItem item)
        {
            return new FlatNotes.Utils.Migration.Versions.v1.Models.ChecklistItem(item.Text, item.IsChecked == true);
        }
    }
}