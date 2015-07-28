using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using Windows.UI.StartScreen;

namespace FlatNotes.Utils.Migration.Versions.v2.Models
{
    public class Notes : ObservableCollection<Note>
    {
        public static implicit operator Notes(FlatNotes.Utils.Migration.Versions.v1.Models.Notes _notes)
        {
            var notes = new Notes();
            foreach (var item in _notes)
                notes.Add(item);

            return notes;
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v1.Models.Notes(Notes _notes)
        {
            var notes = new FlatNotes.Utils.Migration.Versions.v1.Models.Notes();
            foreach (var item in _notes)
                notes.Add(item);

            return notes;
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v2.Models.Notes(List<Note> _notes)
        {
            var notes = new FlatNotes.Utils.Migration.Versions.v2.Models.Notes();
            foreach (var item in _notes)
                notes.Add(item);

            return notes;
        }

        public static implicit operator List<Note>(FlatNotes.Utils.Migration.Versions.v2.Models.Notes _notes)
        {
            var notes = new List<Note>();
            foreach (var item in _notes)
                notes.Add(item);

            return notes;
        }
    }

    [DataContract]
    public class Note : ModelBase
    {
        public string ID { get { return id; } set { id = value; } }
        [DataMember(Name = "_id")]
        private string id = Guid.NewGuid().ToString();

        [IgnoreDataMember]
        public bool Changed { get { return changed; } set { if (changed != value) { changed = value; NotifyPropertyChanged("Changed"); } } }
        private bool changed = false;

        public bool IsChecklist { get { return isChecklist; } set { if (isChecklist != value) { isChecklist = value; if (value) EnableChecklist(); else DisableChecklist(); NotifyPropertyChanged("IsChecklist"); NotifyPropertyChanged("IsText"); } } }
        [DataMember(Name = "IsChecklist")]
        private bool isChecklist;

        [IgnoreDataMember]
        public bool IsText { get { return !IsChecklist; } }

        public string Title { get { return title; } set { if (title != value) { title = value; NotifyPropertyChanged("Title"); } } }
        [DataMember(Name = "Title")]
        private string title;

        public string Text { get { return text; } set { if (text != value) { text = value; NotifyPropertyChanged("Text"); } } }
        [DataMember(Name = "Text")]
        private string text;

        public NoteImages Images { get { return images; } set { images = value; NotifyPropertyChanged("Images"); } }
        [DataMember(Name = "Images")]
        private NoteImages images { get { return _images; } set { replaceNoteImages(value); } }
        private NoteImages _images = new NoteImages();

        public Checklist Checklist { get { return checklist; } private set { checklist = value; NotifyPropertyChanged("Checklist"); } }
        [DataMember(Name = "Checklist")]
        private Checklist checklist { get { return _checklist; } set { replaceChecklist(value); } }
        private Checklist _checklist = new Checklist();

        [IgnoreDataMember]
        public NoteColor Color { get { return color; } set { var newValue = value is NoteColor ? value : NoteColor.DEFAULT; if (color.Key != newValue.Key) { color = newValue; NotifyPropertyChanged("Color"); } } }
        private NoteColor color = NoteColor.DEFAULT;

        [DataMember(Name = "Color")]
        private string _color { get { return Color.Key; } set { color = new NoteColor(value); } }

        public DateTime? CreatedAt { get { return createdAt; } private set { createdAt = value; NotifyPropertyChanged("CreatedAt"); } }
        [DataMember(Name = "CreatedAt")]
        private DateTime? createdAt;

        public DateTime? UpdatedAt { get { return updatedAt; } set { updatedAt = value; NotifyPropertyChanged("UpdatedAt"); } }
        [DataMember(Name = "UpdatedAt")]
        private DateTime? updatedAt;

        public DateTime? ArchivedAt { get { return archivedAt; } set { archivedAt = value; NotifyPropertyChanged("ArchivedAt"); } }
        [DataMember(Name = "ArchivedAt")]
        private DateTime? archivedAt;

        [IgnoreDataMember]
        public bool IsPinned { get { return isPinned; } set { isPinned = value; NotifyPropertyChanged("IsPinned"); } }
        private bool isPinned;

        [IgnoreDataMember]
        public bool AlreadyExists { get { return alreadyExists; } set { alreadyExists = value; IsNewNote = !AlreadyExists && !IsArchived; NotifyPropertyChanged("AlreadyExists"); } }
        private bool alreadyExists;

        [IgnoreDataMember]
        public bool IsArchived { get { return isArchived; } set { isArchived = value; IsNewNote = !AlreadyExists && !IsArchived; NotifyPropertyChanged("IsArchived"); } }
        private bool isArchived;

        [IgnoreDataMember]
        public bool IsNewNote { get { return isNewNote; } private set { isNewNote = value; NotifyPropertyChanged("IsNewNote"); } }
        private bool isNewNote;

        public Note()
        {
            PropertyChanged += Note_PropertyChanged;
            IsPinned = SecondaryTile.Exists(ID);
        }

        public Note(bool isChecklist = false) : this()
        {
            this.isChecklist = isChecklist;
        }

        public Note(string title, string text = "", NoteColor color = null) : this()
        {
            this.title = title;
            this.text = text;
            this.color = color is NoteColor ? color : NoteColor.DEFAULT;
        }

        public Note(string title, Checklist checklist, NoteColor color = null) : this()
        {
            this.title = title;
            this.text = "";
            this.color = color is NoteColor ? color : NoteColor.DEFAULT;

            this.isChecklist = true;
            this.checklist = checklist;
        }

        internal Note(string id, string title, string text, Checklist checklist, NoteImages images, NoteColor color, DateTime? createdAt, DateTime? updatedAt, DateTime? archivedAt) : this()
        {
            this.isChecklist = checklist != null && checklist.Count > 0;

            this.id = id;
            this.title = title;
            this.text = this.isChecklist ? "" : text;
            this.checklist = checklist;
            this.images = images;
            this.color = color is NoteColor ? color : NoteColor.DEFAULT;
            this.createdAt = createdAt;
            this.updatedAt = updatedAt;
            this.archivedAt = archivedAt;
        }

        //public Note(string id, bool isChecklist, string title, string text, Checklist checklist, 
        //notes.Add(new Note() { ID = note.ID, IsChecklist = note.IsChecklist, Title = note.Title, Text = note.Text, Checklist = note.Checklist, Color = note.Color, CreatedAt = note.CreatedAt, UpdatedAt = note.UpdatedAt  });

        void Note_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "IsChecklist"
                || e.PropertyName == "Title" || e.PropertyName == "Text"
                || e.PropertyName == "Checklist" || e.PropertyName == "Images"
                || e.PropertyName == "Color" || e.PropertyName == "UpdatedAt")) return;

            Debug.WriteLine("Note_PropertyChanged " + e.PropertyName);
            Changed = true;

            if (e.PropertyName == "UpdatedAt") return;
            Touch();
        }

        public void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }


        public bool ToggleChecklist()
        {
            if (!this.IsChecklist)
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

            if (!(string.IsNullOrEmpty(Text)))
            {
                string[] lines = Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                Checklist.Clear();
                foreach (string line in lines)
                    Checklist.Add(ChecklistItem.FromText(line));
            }

            Text = "";
        }

        private void DisableChecklist()
        {
            this.isChecklist = false;

            Text = GetTextFromChecklist();

            Checklist.Clear();
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Title) && ((!IsChecklist && string.IsNullOrEmpty(Text)) || (IsChecklist && Checklist.Count <= 0)) && Images.Count <= 0;
        }

        public string GetContent(bool showCheckedMark = false, bool includeTitle = false)
        {
            string content = IsChecklist ? GetTextFromChecklist(showCheckedMark) : Text;
            if (includeTitle && !string.IsNullOrEmpty(Title)) content = Title + Environment.NewLine + content;

            return content;
        }

        public void Trim()
        {
            if (!string.IsNullOrEmpty(Title)) Title = title.Trim();
            if (!string.IsNullOrEmpty(Text)) Text = text.Trim();

            if (IsChecklist && Checklist != null)
                for (int i = Checklist.Count - 1; i >= 0; i--)
                {
                    if (!string.IsNullOrEmpty(Checklist[i].Text)) Checklist[i].text = Checklist[i].text.Trim();

                    if (string.IsNullOrEmpty(Checklist[i].Text))
                        Checklist.RemoveAt(i);
                }
        }

        protected string GetTextFromChecklist(bool showCheckedMark = false)
        {
            string txt = "";

            foreach (ChecklistItem item in Checklist)
                txt += item.ToString(showCheckedMark) + Environment.NewLine.ToString();

            return txt.Trim();
        }

        private void replaceNoteImages(IList<NoteImage> list)
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
            if (Checklist.Count <= 0 && (list == null || list.Count <= 0))
                return;

            Checklist.Clear();

            if (list == null || list.Count <= 0)
                return;

            foreach (var item in list)
                Checklist.Add(item);

            return;
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v1.Models.Note(Note _note)
        {
            return new FlatNotes.Utils.Migration.Versions.v1.Models.Note(_note.ID, _note.Title, _note.Text, (Checklist)_note.Checklist, _note.Color, _note.CreatedAt, _note.UpdatedAt);
        }

        public static implicit operator Note(FlatNotes.Utils.Migration.Versions.v1.Models.Note _note)
        {
            return new Note(_note.ID, _note.Title, _note.Text, (Checklist)_note.Checklist, _note.Images, _note.Color, _note.CreatedAt, _note.UpdatedAt, null);
        }
    }
}