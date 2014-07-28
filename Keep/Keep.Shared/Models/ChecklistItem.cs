using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Keep.Models
{
    [DataContract]
    public class ChecklistItem : BaseModel
    {
        [DataMember]
        public String Text { get { return text; } set { if ( text != value ) { text = value; NotifyPropertyChanged( "Text" ); } } }
        private String text;

        [DataMember]
        public bool IsChecked { get { return isChecked; } set { if ( isChecked != value ) { isChecked = value; NotifyPropertyChanged( "IsChecked" ); } } }
        private bool isChecked;

        public ChecklistItem( String text = "", bool isChecked = false )
        {
            this.Text = text;
            this.IsChecked = isChecked;
        }
    }
}
