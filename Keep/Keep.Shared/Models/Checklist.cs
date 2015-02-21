using Keep.Common;
using System.Runtime.Serialization;

namespace Keep.Models
{
    public class Checklist : TrulyObservableCollection<ChecklistItem>
    {
        public Checklist() : base(false) { }
    }

    [DataContract]
    public class ChecklistItem : ModelBase
    {
        public const char CHECKED_SYMBOL = '☑';//▣
        public const char UNCHECKED_SYMBOL = '⬜';

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
            string checkMark = showCheckedMark ? (IsChecked ? CHECKED_SYMBOL + " " : UNCHECKED_SYMBOL + " ") : "";
            string str = checkMark + Text;

            return str.Trim();
        }
    }
}