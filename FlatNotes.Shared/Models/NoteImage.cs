using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Windows.Foundation;

namespace FlatNotes.Models
{
    public class NoteImages : ObservableCollection<NoteImage>
    {
        public static implicit operator NoteImages(FlatNotes.Utils.Migration.Versions.v2.Models.NoteImages _noteImages)
        {
            var noteImages = new NoteImages();
            foreach (var item in _noteImages)
                noteImages.Add(item);

            return noteImages;
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v2.Models.NoteImages(NoteImages _noteImages)
        {
            var noteImages = new FlatNotes.Utils.Migration.Versions.v2.Models.NoteImages();
            foreach (var item in _noteImages)
                noteImages.Add(item);

            return noteImages;
        }

        public static implicit operator List<NoteImage>(NoteImages _noteImages)
        {
            var noteImages = new List<NoteImage>();
            foreach (var item in _noteImages)
                noteImages.Add(item);

            return noteImages;
        }

        public static implicit operator NoteImages(List<NoteImage> _noteImages)
        {
            var noteImages = new NoteImages();
            foreach (var item in _noteImages)
                noteImages.Add(item);

            return noteImages;
        }
    }

    [DataContract]
    public class NoteImage : ModelBase
    {
        [PrimaryKey]
        public string ID { get { return id; } set { id = value; } }
        [DataMember(Name = "_id")]
        private string id = Guid.NewGuid().ToString();

        [ForeignKey(typeof(Note))]
        public string NoteId { get; set; }

        public String URL { get { return url; } set { if ( url != value ) { url = value; NotifyPropertyChanged("URL"); } } }
        [DataMember(Name = "URL")]
        private String url;

        public double Width { get { return width; } set { width = value; NotifyPropertyChanged("Width"); } }
        [DataMember(Name = "Width")]
        private double width;

        public double Height { get { return height; } set { height = value; NotifyPropertyChanged("Height"); } }
        [DataMember(Name = "Height")]
        private double height;

        [DataMember]
        public Double Proportion { get { return Height / Width; } }

        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }
        [DataMember(Name = "IsSelected")]
        private bool isSelected = false;

        public DateTime CreatedAt { get { return createdAt; } private set { createdAt = value; } }
        [DataMember(Name = "CreatedAt")]
        private DateTime createdAt = DateTime.Now;

        public NoteImage() { }

        public NoteImage(string url)
        {
            this.URL = url;
        }

        public NoteImage(string url, double width, double height)
        {
            this.URL = url;
            this.width = width;
            this.height = height;
        }

        public NoteImage(string url, Size size) : this(url, size.Width, size.Height)
        {
        }

        public static implicit operator NoteImage(FlatNotes.Utils.Migration.Versions.v2.Models.NoteImage noteImage)
        {
            return new NoteImage(noteImage.URL, noteImage.Size);
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v2.Models.NoteImage(NoteImage noteImage)
        {
            return new FlatNotes.Utils.Migration.Versions.v2.Models.NoteImage(noteImage.URL, noteImage.Width, noteImage.Height);
        }
    }
}