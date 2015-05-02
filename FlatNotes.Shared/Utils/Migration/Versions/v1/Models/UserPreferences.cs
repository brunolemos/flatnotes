using System;
using System.Runtime.Serialization;
using Windows.UI.Xaml;
using System.Diagnostics;

namespace FlatNotes.Utils.Migration.Versions.v1.Models
{
    [DataContract]
    public class UserPreferences : BaseModel
    {
        public const Double ITEM_MIN_WIDTH_MIN_VALUE = 100;
        public const Double ITEM_MIN_WIDTH_MAX_VALUE = 600;
        public const Double ITEM_MIN_WIDTH_DEFAULT_VALUE = 150;

        public int Columns { get { return columns; } set { Debug.WriteLine("Changed columns to: " + value); if (columns != value) { columns = value; NotifyPropertyChanged("Columns"); } } }
        [DataMember(Name = "Columns")]
        private int columns = 2;

        //public Double ItemMinWidth { get { return itemMinWidth; } set { double new_value = Math.Min( Math.Max( value, ITEM_MIN_WIDTH_MIN_VALUE ), ITEM_MIN_WIDTH_MAX_VALUE ); if ( itemMinWidth != new_value ) { itemMinWidth = new_value; NotifyPropertyChanged( "ItemMinWidth" ); } } }
        //[DataMember(Name = "ItemMinWidth")]
        //private Double itemMinWidth = ITEM_MIN_WIDTH_DEFAULT_VALUE;

        public ElementTheme Theme { get { return theme; } set { if (theme != value) { theme = value; NotifyPropertyChanged("Theme"); } } }
        [DataMember(Name = "Theme")]
        private ElementTheme theme = ElementTheme.Light;

        public UserPreferences()
        {
            //PropertyChanged += UserPreferences_PropertyChanged;
        }

        //void UserPreferences_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    Debug.WriteLine("UserPreferences_PropertyChanged " + e.PropertyName);
        //}
    }
}
