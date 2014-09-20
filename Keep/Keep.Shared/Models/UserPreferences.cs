using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Windows.UI.Xaml;
using System.Diagnostics;

namespace Keep.Models
{
    [DataContract]
    public class UserPreferences : BaseModel
    {
        public const Double ITEM_MIN_WIDTH_MIN_VALUE = 100;
        public const Double ITEM_MIN_WIDTH_MAX_VALUE = 600;
        public const Double ITEM_MIN_WIDTH_DEFAULT_VALUE = 200;

        [DataMember]
        public int Columns { get { return columns; } set { Debug.WriteLine("Changed columns to: " + value); if (columns != value) { columns = value; NotifyPropertyChanged("Columns"); } } }
        private int columns = -1;

        [DataMember]
        public Double ItemMinWidth { get { return itemMinWidth; } set { double new_value = Math.Min( Math.Max( value, ITEM_MIN_WIDTH_MIN_VALUE ), ITEM_MIN_WIDTH_MAX_VALUE ); if ( itemMinWidth != new_value ) { itemMinWidth = new_value; NotifyPropertyChanged( "ItemMinWidth" ); } } }
        private Double itemMinWidth = ITEM_MIN_WIDTH_DEFAULT_VALUE;

        [DataMember]
        public ElementTheme Theme { get { return theme; } set { theme = value; NotifyPropertyChanged("Theme"); } }
        private ElementTheme theme = ElementTheme.Light;

        //[DataMember]
        //public string Language { get { return language; } }
        //private string language = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.CultureName;
    }
}
