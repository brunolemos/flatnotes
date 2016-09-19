using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Windows.Foundation;

namespace FlatNotes.Utils.Migration.Versions.v2.Models
{
    public class NoteImages : ObservableCollection<NoteImage>
    {
        public static implicit operator NoteImages(FlatNotes.Utils.Migration.Versions.v1.Models.NoteImages _noteImages)
        {
            var noteImages = new NoteImages();
            foreach (var item in _noteImages)
                noteImages.Add(item);

            return noteImages;
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v1.Models.NoteImages(NoteImages _noteImages)
        {
            var noteImages = new FlatNotes.Utils.Migration.Versions.v1.Models.NoteImages();
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
        public String URL { get { return url; } set { if ( url != value ) { url = value; NotifyPropertyChanged("URL"); } } }
        [DataMember(Name = "URL")]
        private String url;

        public Size Size { get { return size; } set { size = value; NotifyPropertyChanged("Size"); } }
        [DataMember(Name = "Size")]
        private Size size;

        [DataMember]
        public Double Proportion { get { return Size.Height / Size.Width; } }

        public DateTime CreatedAt { get { return createdAt; } private set { createdAt = value; } }
        [DataMember(Name = "CreatedAt")]
        private DateTime createdAt = DateTime.UtcNow;

        public NoteImage() { }

        public NoteImage(string url)
        {
            this.url = url;
        }

        public NoteImage(string url, Size size)
        {
            this.url = url;
            this.size = size;
        }

        public NoteImage(string url, double width, double height) : this(url, new Size(width, height))
        {
        }

        public static implicit operator NoteImage(FlatNotes.Utils.Migration.Versions.v1.Models.NoteImage noteImage)
        {
            return new NoteImage(noteImage.URL, noteImage.Size);
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v1.Models.NoteImage(NoteImage noteImage)
        {
            return new FlatNotes.Utils.Migration.Versions.v1.Models.NoteImage(noteImage.URL, noteImage.Size);
        }
    }
}