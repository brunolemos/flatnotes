using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Runtime.Serialization;

namespace FlatNotes.Models
{
    [DataContract]
    public class Reminder : ModelBase
    {
        [PrimaryKey]
        public string ID { get { return id; } set { id = value; } }
        [DataMember(Name = "ID")]
        private string id = Guid.NewGuid().ToString();

        [ForeignKey(typeof(Note))]
        [DataMember(Name = "NoteId")]
        public string NoteId { get; set; }

        public bool IsActive { get { return isActive; } set { if (isActive == value) return; isActive = value; NotifyPropertyChanged("IsActive"); NotifyChanges(); } }
        [DataMember(Name = "IsActive")]
        private bool isActive = false;

        public DateTimeOffset? Date { get { return date; } set { date = value; NotifyPropertyChanged("Date"); NotifyPropertyChanged("FormatedString"); NotifyChanges(); } }
        [DataMember(Name = "Date")]
        private DateTimeOffset? date = null;

        public DateTime CreatedAt { get { return createdAt; } private set { createdAt = value; } }
        [DataMember(Name = "CreatedAt")]
        private DateTime createdAt = DateTime.UtcNow;

        public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; } }
        [DataMember(Name = "UpdatedAt")]
        private DateTime updatedAt = DateTime.UtcNow;

        public string FormatedString { get { return date?.DateTime == null ? "" : date?.LocalDateTime.ToString("MMMM dd, yyyy HH:mm"); } }

        public void ResetIfIsPastDate()
        {
           if (date == null || !date.HasValue || (date.Value.ToLocalTime() < DateTimeOffset.Now))
            {
                System.Diagnostics.Debug.WriteLine("Date {0} < {1}", date, DateTimeOffset.Now);
                isActive = false;
                date = null;
            }
        }

        public Reminder() {
        }

        public Reminder(DateTimeOffset? date = null) : this()
        {
            if (date != null && date.HasValue && date.Value != null) this.Date = date.Value.ToLocalTime();
        }
        
        public void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public static string NoteIdToReminderID(string noteId)
        {
            return noteId.Substring(0, 15);
        }
    }
}