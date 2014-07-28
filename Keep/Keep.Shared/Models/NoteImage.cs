using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Runtime.Serialization;
using Windows.Foundation;

using Keep.Utils;

namespace Keep.Models
{
    [DataContract]
    public class NoteImage : BaseModel
    {
        public bool Show { get { return show; } set { if (value != show) { show = value; NotifyPropertyChanged("Show"); } } }
        private bool show = true;

        [DataMember]
        public String URL { get { return url; } set { if ( value != url ) { Show = true; url = value; NotifyPropertyChanged( "URL" ); } } }
        private String url;

        [DataMember]
        public Size Size { get { return size; } set { size = value; } }
        private Size size;

        [DataMember]
        public Double Proportion { get { return Size.Height / Size.Width; } }

        [DataMember]
        public DateTime CreatedAt { get { return createdAt; } private set { createdAt = value; } }
        private DateTime createdAt = DateTime.Now;

        public NoteImage() { }

        public NoteImage(string url)
        {
            this.URL = url;
        }

        public NoteImage(string url, Size size)
        {
            this.URL = url;
            this.Size = size;
        }
    }
}