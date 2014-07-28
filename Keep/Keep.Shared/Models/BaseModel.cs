using System;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Keep.Models
{
    public abstract class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged( String propertyName )
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if ( null != handler )
            {
                handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        public void NotifyChanges()
        {
            NotifyPropertyChanged( String.Empty );
        }
    }
}