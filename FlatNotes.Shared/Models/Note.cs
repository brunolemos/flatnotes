using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Windows.UI.StartScreen;

namespace FlatNotes.Models
{
    public class Notes : ObservableCollection<Note>
    {
        public static implicit operator Notes(FlatNotes.Utils.Migration.Versions.v2.Models.Notes _notes)
        {
            var notes = new Notes();
            foreach (var item in _notes)
                notes.Add(item);

            return notes;
        }

        public static implicit operator Notes(List<Note> _notes)
        {
            var notes = new Notes();
            foreach (var item in _notes)
                notes.Add(item);

            return notes;
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v2.Models.Notes(Notes _notes)
        {
            var notes = new FlatNotes.Utils.Migration.Versions.v2.Models.Notes();
            foreach (var item in _notes)
                notes.Add(item);

            return notes;
        }
    }

    [DataContract]
    public class Note : ModelBase
    {
        [PrimaryKey]
        public string ID { get { return id; } set { id = value; } }
        [DataMember(Name = "_id")]
        private string id = Guid.NewGuid().ToString();

        [IgnoreDataMember]
        [Ignore]
        public bool Changed { get { return changed; } set { if (changed != value) { changed = value; NotifyPropertyChanged("Changed"); } } }
        private bool changed = false;

        public int Order { get { return order; } set { if (order != value) { order = value; NotifyPropertyChanged("Order"); } } }
        [DataMember(Name = "Order")]
        private int order;

        public bool IsArchived { get { return isArchived; } set { isArchived = value; NotifyPropertyChanged("IsArchived"); } }
        [DataMember(Name = "IsArchived")]
        private bool isArchived;

        public bool IsChecklist { get { return isChecklist; } set { if (isChecklist != value) { isChecklist = value; if (value) EnableChecklist(); else DisableChecklist(); NotifyPropertyChanged("IsChecklist"); NotifyPropertyChanged("IsText"); } } }
        [DataMember(Name = "IsChecklist")]
        private bool isChecklist;

        [IgnoreDataMember]
        [Ignore]
        public bool IsText { get { return !IsChecklist; } }

        public string Title { get { return title; } set { if (title != value) { title = value; NotifyPropertyChanged("Title"); } } }
        [DataMember(Name = "Title")]
        private string title = "";

        public string Text { get { return text; } set { if (text != value) { text = value; NotifyPropertyChanged("Text"); } } }
        [DataMember(Name = "Text")]
        private string text = "";

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<NoteImage> _ignore1 { get { return images; } set { images = value; } }

        [Ignore]
        public NoteImages Images { get { return images; } private set { images = value; NotifyPropertyChanged("Images"); } }
        [DataMember(Name = "Images")]
        private NoteImages images { get { return _images; } set { replaceNoteImages(value); } }
        private NoteImages _images = new NoteImages();

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<ChecklistItem> _ignore2 { get { return checklist; } set { checklist = value; } }

        [Ignore]
        public Checklist Checklist { get { return checklist; } private set { checklist = value; NotifyPropertyChanged("Checklist"); } }
        [DataMember(Name = "Checklist")]
        private Checklist checklist { get { return _checklist; } set { replaceChecklist(value); } }
        private Checklist _checklist = new List<ChecklistItem>();

        [IgnoreDataMember]
        [Ignore]
        public NoteColor Color { get { return color; } set { var newValue = value is NoteColor ? value : NoteColor.DEFAULT; if (color.Key != newValue.Key) { color = newValue; NotifyPropertyChanged("Color"); } } }
        private NoteColor color = NoteColor.DEFAULT;

        [DataMember(Name = "Color")]
        [Column("Color")]
        public string _color { get { return Color.Key; } set { color = new NoteColor(value); } }

        public DateTime? CreatedAt { get { return createdAt; } protected set { createdAt = value; NotifyPropertyChanged("CreatedAt"); } }
        [DataMember(Name = "CreatedAt")]
        private DateTime? createdAt;

        public DateTime? UpdatedAt { get { return updatedAt; } protected set { updatedAt = value; NotifyPropertyChanged("UpdatedAt"); } }
        [DataMember(Name = "UpdatedAt")]
        private DateTime? updatedAt;

        public DateTime? ArchivedAt { get { return archivedAt; } protected set { archivedAt = value; NotifyPropertyChanged("ArchivedAt"); } }
        [DataMember(Name = "ArchivedAt")]
        private DateTime? archivedAt;

        [IgnoreDataMember]
        [Ignore]
        public bool IsPinned { get { return SecondaryTile.Exists(ID); } }

        [IgnoreDataMember]
        [Ignore]
        public bool CanPin { get { return !IsPinned && !IsArchived; } }

        public Note()
        {
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

        public Note(string title, List<ChecklistItem> checklist, NoteColor color = null) : this()
        {
            this.title = title;
            this.text = "";
            this.color = color is NoteColor ? color : NoteColor.DEFAULT;

            this.isChecklist = true;
            this.checklist = checklist;
        }

        internal Note(string id, string title, string text, List<ChecklistItem> checklist, List<NoteImage> images, NoteColor color, DateTime? createdAt, DateTime? updatedAt, DateTime? archivedAt) : this()
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
        
        public void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public void TouchCreatedAt()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public void TouchArchivedAt()
        {
            ArchivedAt = DateTime.UtcNow;
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

            Text = GetTextFromChecklist(false);

            Checklist.Clear();
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Title) && ((!IsChecklist && string.IsNullOrEmpty(Text)) || (IsChecklist && Checklist.Count <= 0)) && Images.Count <= 0;
        }

        public string GetContent(bool showCheckedMark = false, bool includeTitle = false, int maxChecklistItem = -1)
        {
            string content = IsChecklist ? GetTextFromChecklist(showCheckedMark, maxChecklistItem) : Text;
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

        protected string GetTextFromChecklist(bool showCheckedMark, int maxChecklistItem = -1)
        {
            string txt = "";
            if (Checklist == null || Checklist.Count <= 0) return txt;

            maxChecklistItem = maxChecklistItem <= 0 ? Checklist.Count : Math.Min(Checklist.Count, maxChecklistItem);
            for (int i = 0; i < maxChecklistItem; i++)
                txt += Checklist[i].ToString(showCheckedMark) + Environment.NewLine.ToString();

            return txt.Trim();
        }

        private void replaceNoteImages(IList<NoteImage> list)
        {
            Images.Clear();

            if (list == null || list.Count <= 0)
                return;

            foreach (var item in list)
            {
                item.NoteId = ID;
                Images.Add(item);
            }

            return;
        }

        private void replaceChecklist(IList<ChecklistItem> list)
        {
            Checklist.Clear();

            if (list == null || list.Count <= 0)
                return;

            foreach (var item in list)
            {
                item.NoteId = ID;
                Checklist.Add(item);
            }

            return;
        }

        public static implicit operator Note(FlatNotes.Utils.Migration.Versions.v2.Models.Note _note)
        {
            var note = new Note(_note.ID, _note.Title, _note.Text, ((Checklist)_note.Checklist).ToList(), (NoteImages)_note.Images, _note.Color, _note.CreatedAt, _note.UpdatedAt, _note.ArchivedAt);

            foreach (var item in note.Checklist)
                item.NoteId = note.ID;

            foreach (var item in note.Images)
                item.NoteId = note.ID;

            return note;
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v2.Models.Note(Note _note)
        {
            return new FlatNotes.Utils.Migration.Versions.v2.Models.Note(_note.ID, _note.Title, _note.Text, (Checklist)_note.Checklist, (NoteImages)_note.Images, _note.Color, _note.CreatedAt, _note.UpdatedAt, _note.ArchivedAt);
        }
    }
}