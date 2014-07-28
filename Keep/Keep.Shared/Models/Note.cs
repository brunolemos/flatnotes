using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Runtime.Serialization;
using Keep.Models.Interfaces;

using Keep.Utils;

namespace Keep.Models
{
    [DataContract]
    public class Note : BaseModel, IIdentifiableModelInterface
    {
        public bool Changed = false;
        public String GetID() { return ID; }
        public DateTime GetCreatedAt() { return CreatedAt; }
        public DateTime GetUpdatedAt() { return UpdatedAt; }

        [DataMember]
        public String ID { get { return id; } private set { id = value; } }
        private String id = GenerateRandomID.Generate();

        [DataMember]
        public bool IsChecklist { get { return isChecklist; } set { if ( isChecklist != value ) { isChecklist = value; if ( value ) EnableChecklist(); else DisableChecklist(); NotifyPropertyChanged( "IsChecklist" ); } } }
        private bool isChecklist;

        [DataMember]
        public String Title { get { return title; } set { if ( title != value ) { title = value; NotifyPropertyChanged( "Title" ); } } }
        private String title;

        [DataMember]
        public String Text { get { return text; } set { if (text != value) { text = value; NotifyPropertyChanged("Text"); } } }
        private String text;

        [DataMember]
        public NoteImages Images { get { return images; } set { replaceNoteImages(value); NotifyPropertyChanged("Images"); } }
        private NoteImages images = new NoteImages();

        [DataMember]
        public Checklist Checklist { get { return checklist; } set { replaceChecklist(value); NotifyPropertyChanged("Checklist"); } }
        private Checklist checklist = new Checklist();

        [IgnoreDataMember]
        public NoteColor Color { get { return color; } set { _colortmp = value is NoteColor ? value : NoteColor.DEFAULT; if ( color.Key != _colortmp.Key ) { color = _colortmp; NotifyPropertyChanged( "Color" ); } } }
        private NoteColor color = NoteColor.DEFAULT;
        private NoteColor _colortmp = NoteColor.DEFAULT;

        [DataMember( Name="Color" )]
        private string _color { get { return Color.Key; } set { color = new NoteColor(value); } }

        [DataMember]
        public DateTime CreatedAt { get { return createdAt; } private set { createdAt = value; } }
        private DateTime createdAt = DateTime.Now;

        [DataMember]
        public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; } }
        private DateTime updatedAt = DateTime.Now;
        
        public Note() {
            PropertyChanged += Note_PropertyChanged;
            Checklist.CollectionChanged += Checklist_CollectionChanged;
        }

        public Note( string title = "", string text = "", NoteColor color = null ) : base()
        {
            this.IsChecklist = false;
            this.Title = title;
            this.Text = text;
            this.Color = color;
        }

        public Note( string title, Checklist checklist, NoteColor color = null ) : base()
        {
            this.IsChecklist = true;
            this.Checklist = checklist;

            this.Title = title;
            this.Text = "";
            this.Color = color;
        }

        void Note_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Changed = true;
        }

        void Checklist_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("Checklist collection changed");
        }

        public void Touch()
        {
            UpdatedAt = DateTime.Now;
        }


        public bool ToggleChecklist()
        {
            if ( !this.IsChecklist )
            {
                this.IsChecklist = true;
                return true;
            }
            else
            {
                this.IsChecklist = false;
                return false;
            }
        }

        private void EnableChecklist()
        {
            this.isChecklist = true;

            if ( !( string.IsNullOrEmpty( Text ) ) )
            {
                string[] lines = Text.Split( Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries );

                this.Checklist = new Checklist();
                foreach ( string line in lines )
                    Checklist.Add ( new ChecklistItem( line ) );
            }

            Text = "";
        }

        private void DisableChecklist()
        {
            Debug.WriteLine("DisableChecklist " + Checklist.Count);
            this.isChecklist = false;

            Text = GetTextFromChecklist();

            Checklist.Clear();
        }

        public String GetContent()
        {
            return IsChecklist ? GetTextFromChecklist() : Text;
        }

        public String GetTextFromChecklist()
        {
            string txt = "";

            foreach (ChecklistItem item in Checklist)
                txt += item.Text + Environment.NewLine.ToString();

            return txt.Trim();
        }

        private void replaceNoteImages(NoteImages list)
        {
            Images.Clear();

            if (list == null || list.Count <= 0)
                return;

            foreach (var item in list)
                Images.Add(item);

            return;
        }

        private void replaceChecklist(Checklist list)
        {
            Checklist.Clear();

            if (list == null || list.Count <= 0)
                return;

            foreach (var item in list)
                Checklist.Add(item);

            return;
        }
    }
}