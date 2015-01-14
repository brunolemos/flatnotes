using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Keep.Models
{
    public class Notes : ObservableCollection<Note> { }

    [DataContract]
    public class Note : ModelBase
    {
        public bool Changed = false;

        //public bool IsPinned { get { isPinned = SecondaryTile.Exists(this.ID); return isPinned; } set { if (isPinned != value) { isPinned = value; NotifyPropertyChanged("IsPinned"); } } }
        //private bool isPinned { get { return isPinned_value; } set { if (isPinned_value != value) { isPinned_value = value; NotifyPropertyChanged("IsPinned"); } } }
        //private bool isPinned_value;

        public string ID { get { return id; } private set { id = value; } }
        [DataMember(Name = "ID")]
        private string id = Guid.NewGuid().ToString();

        public bool IsChecklist { get { return isChecklist; } set { if (isChecklist != value) { isChecklist = value; if (value) EnableChecklist(); else DisableChecklist(); NotifyPropertyChanged("IsChecklist"); NotifyPropertyChanged("IsText"); } } }
        [DataMember(Name = "IsChecklist")]
        private bool isChecklist;

        [IgnoreDataMember]
        public bool IsText { get { return !IsChecklist; } }

        public String Title { get { return title; } set { if (title != value) { title = value; NotifyPropertyChanged("Title"); } } }
        [DataMember(Name = "Title")]
        private String title;

        public String Text { get { return text; } set { if (text != value) { text = value; NotifyPropertyChanged("Text"); } } }
        [DataMember(Name = "Text")]
        private String text;

        //public NoteImages Images { get { return images; } set { replaceNoteImages(value); NotifyPropertyChanged("Images"); } }
        //[DataMember(Name = "Images")]
        //private NoteImages images = new NoteImages();

        public Checklist Checklist { get { return checklist; } private set { replaceChecklist(value); NotifyPropertyChanged("Checklist"); } }
        [DataMember(Name = "Checklist")]
        private Checklist checklist = new Checklist();

        [IgnoreDataMember]
        public NoteColor Color { get { return color; } set { color = value is NoteColor ? value : NoteColor.DEFAULT; NotifyPropertyChanged("Color"); } }
        private NoteColor color = NoteColor.DEFAULT;

        [DataMember(Name = "Color")]
        private string _color { get { return Color.Key; } set { color = new NoteColor(value); } }

        public DateTime CreatedAt { get { return createdAt; } private set { createdAt = value; } }
        [DataMember(Name = "CreatedAt")]
        private DateTime createdAt = DateTime.Now;

        public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; } }
        [DataMember(Name = "UpdatedAt")]
        private DateTime updatedAt = DateTime.Now;

        public Note()
        {
            PropertyChanged += Note_PropertyChanged;
            Checklist.CollectionChanged += (s, e) => NotifyPropertyChanged("Checklist");
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

        void Note_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine("Note_PropertyChanged " + e.PropertyName);
            Changed = true;

            Touch();
        }

        public void Touch()
        {
            UpdatedAt = DateTime.Now;
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
                    Checklist.Add(new ChecklistItem(line));
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
            return String.IsNullOrEmpty(Title) && ((!IsChecklist && String.IsNullOrEmpty(Text)) || (IsChecklist && Checklist.Count <= 0));// && Images.Count <= 0;
        }

        public String GetContent()
        {
            return IsChecklist ? GetTextFromChecklist() : Text;
        }

        public void Trim()
        {
            if (!String.IsNullOrEmpty(Title)) Title = Title.Trim();
            if (!String.IsNullOrEmpty(Text)) Text = Text.Trim();

            if (IsChecklist && Checklist != null)
                for (int i = Checklist.Count - 1; i >= 0; i--)
                {
                    if (!String.IsNullOrEmpty(Checklist[i].Text)) Checklist[i].Text = Checklist[i].Text.Trim();

                    if (String.IsNullOrEmpty(Checklist[i].Text))
                        Checklist.RemoveAt(i);
                }
        }

        protected String GetTextFromChecklist()
        {
            string txt = "";

            foreach (ChecklistItem item in Checklist)
                txt += item.Text + Environment.NewLine.ToString();

            return txt.Trim();
        }

        //private void replaceNoteImages(NoteImages list)
        //{
        //    Images.Clear();

        //    if (list == null || list.Count <= 0)
        //        return;

        //    foreach (var item in list)
        //        Images.Add(item);

        //    return;
        //}

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