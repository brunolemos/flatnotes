using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Windows.Foundation;

namespace FlatNotes.Utils.Migration.Versions.v1.Models
{
    public class NoteImages : ObservableCollection<NoteImage> { }

    [DataContract]
    public class NoteImage : BaseModel
    {
        public bool Show { get { return show; } set { if (value != show) { show = value; NotifyPropertyChanged("Show"); } } }
        private bool show = true;

        public String URL { get { return url; } set { if ( value != url ) { Show = true; url = value; NotifyPropertyChanged( "URL" ); } } }
        [DataMember(Name = "URL")]
        private String url;

        public Size Size { get { return size; } set { size = value; } }
        [DataMember(Name = "Size")]
        private Size size;

        [DataMember]
        public Double Proportion { get { return Size.Height / Size.Width; } }

        public DateTime CreatedAt { get { return createdAt; } private set { createdAt = value; } }
        [DataMember(Name = "CreatedAt")]
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