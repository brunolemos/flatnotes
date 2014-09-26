using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Keep.Controllers;
using Keep.Models.Interfaces;

namespace Keep.Models
{
    public class Users : List<User> { }

    [DataContract]
    public class User : BaseModel, IIdentifiableModelInterface
    {
        public String GetID() { return AnonymousID; }
        public DateTime GetCreatedAt() { return CreatedAt; }
        public DateTime GetUpdatedAt() { return UpdatedAt; }

        public string AnonymousID { get { return anonymousID; } }
        [DataMember(Name = "AnonymousID")]
        private string anonymousID = DeviceController.UserAnonymousID;

        public Devices Devices { get { return devices; } set { replaceDevices(value); } }
        [DataMember(Name = "DevicesList")]
        private Devices devices = new Devices();

        public String Name { get { return name; } set { if ( name != value ) { name = value; NotifyPropertyChanged( "Name" ); } } }
        [DataMember(Name = "Name")]
        private String name;

        public String Email { get { return email; } set { if ( email != value ) { email = value; NotifyPropertyChanged( "Email" ); } } }
        [DataMember(Name = "Email")]
        private String email;

        public Notes Notes { get { return notes; } set { replaceNotes(value); } }
        [DataMember(Name = "Notes")]
        private Notes notes = new Notes();

        public Notes ArchivedNotes { get { return archivedNotes; } set { replaceArchivedNotes(value); } }
        [DataMember(Name = "ArchivedNotes")]
        private Notes archivedNotes = new Notes();

        public UserPreferences Preferences { get { return preferences; } set { preferences = value; NotifyPropertyChanged( "Preferences" ); } }
        [DataMember(Name = "Preferences")]
        private UserPreferences preferences = new UserPreferences();

        [DataMember]
        public DateTime CreatedAt = DateTime.Now;

        [DataMember]
        public DateTime UpdatedAt;

        [DataMember]
        public DateTime LastSeenAt = DateTime.Now;

        public User() {
            PropertyChanged += User_PropertyChanged;

            Notes.CollectionChanged += (s, e) => NotifyPropertyChanged("Notes");
            ArchivedNotes.CollectionChanged += (s, e) => NotifyPropertyChanged("ArchivedNotes");
            Preferences.PropertyChanged += (s, e) => NotifyPropertyChanged("Preferences");
        }

        void User_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine(String.Format("User {0} changed", e.PropertyName));
        }

        public void Touch()
        {
            UpdatedAt = DateTime.Now;
        }

        private void replaceDevices(Devices list)
        {
            devices.Clear();

            if (list == null || list.Count <= 0)
                return;

            foreach (var item in list)
                devices.Add(item);

            NotifyPropertyChanged("Devices"); 

            return;
        }

        private void replaceNotes(Notes list)
        {
            Debug.WriteLine("REPLACE NOTES");
            notes.Clear();

            if (list == null || list.Count <= 0)
                return;

            foreach (var item in list)
                notes.Add(item);

            NotifyPropertyChanged("Notes");

            return;
        }

        private void replaceArchivedNotes(Notes list)
        {
            archivedNotes.Clear();

            if (list == null || list.Count <= 0)
                return;

            foreach (var item in list)
                archivedNotes.Add(item);

            NotifyPropertyChanged("ArchivedNotes"); 

            return;
        }
    }
}
