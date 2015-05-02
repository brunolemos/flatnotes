using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace FlatNotes.Models
{
    public class Notes : ObservableCollection<Note> { }

    [DataContract]
    public class Note : ModelBase
    {
        [IgnoreDataMember]
        public bool Changed { get { return changed; } set { if (changed != value) { changed = value; NotifyPropertyChanged("Changed"); } } }
        private bool changed = false;

        public string ID { get { return id; } private set { id = value; } }
        [DataMember(Name = "_id")]
        private string id = Guid.NewGuid().ToString();

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

        public DateTime CreatedAt { get { return createdAt; } private set { createdAt = value; NotifyPropertyChanged("CreatedAt"); } }
        [DataMember(Name = "CreatedAt")]
        private DateTime createdAt = DateTime.UtcNow;

        public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; NotifyPropertyChanged("UpdatedAt"); } }
        [DataMember(Name = "UpdatedAt")]
        private DateTime updatedAt = DateTime.UtcNow;

        public DateTime? ArchivedAt { get { return archivedAt; } set { archivedAt = value; NotifyPropertyChanged("ArchivedAt"); } }
        [DataMember(Name = "ArchivedAt")]
        private DateTime? archivedAt;

        public Note()
        {
            PropertyChanged += Note_PropertyChanged;
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

        public Note(string title, string text, Checklist checklist, NoteColor color, DateTime createdAt, DateTime updatedAt, DateTime? archivedAt) : this()
        {
            this.title = title;
            this.text = text;
            this.checklist = checklist;
            this.color = color is NoteColor ? color : NoteColor.DEFAULT;
            this.createdAt = createdAt;
            this.updatedAt = updatedAt;
            this.archivedAt = archivedAt;

            this.isChecklist = checklist != null;
        }

        //public Note(string id, bool isChecklist, string title, string text, Checklist checklist, 
        //notes.Add(new Note() { ID = note.ID, IsChecklist = note.IsChecklist, Title = note.Title, Text = note.Text, Checklist = note.Checklist, Color = note.Color, CreatedAt = note.CreatedAt, UpdatedAt = note.UpdatedAt  });


        void Note_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == "Changed" || e.PropertyName == "ArchivedAt" || e.PropertyName == "CreatedAt") return;
            Changed = true;

            Debug.WriteLine("Note_PropertyChanged " + e.PropertyName);

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
            if (Checklist.Count <= 0 && (list == null || list.Count <= 0))
                return;

            Checklist.Clear();

            if (list == null || list.Count <= 0)
                return;

            foreach (var item in list)
                Checklist.Add(item);

            return;
        }
    }
}