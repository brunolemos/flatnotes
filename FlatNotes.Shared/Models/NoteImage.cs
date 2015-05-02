using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Windows.Foundation;
using FlatNotes.Common;
using System.Net;

namespace FlatNotes.Models
{
    public class NoteImages : ObservableCollection<NoteImage> { }

    [DataContract]
    public class NoteImage : ModelBase
    {
        public string ID { get { return id; } private set { id = value; } }
        [DataMember(Name = "_id")]
        private string id = Guid.NewGuid().ToString();

        public String URL { get { return url; } set { if ( url != value ) { url = value; NotifyPropertyChanged( "URL" ); } } }
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