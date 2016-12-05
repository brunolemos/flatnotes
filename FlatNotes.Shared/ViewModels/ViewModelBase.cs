using FlatNotes.Common;
using FlatNotes.Utils;
using FlatNotes.Views;
using System;
using Windows.Foundation.Metadata;

namespace FlatNotes.ViewModels
{
    public class ViewModelBase : Notifiable
    {
        public static event EventHandler CloseNoteEvent;

        public bool IsLoaded { get { return isLoaded; } set { isLoaded = value; NotifyPropertyChanged("IsLoaded"); } }
        private bool isLoaded = false;

        public bool IsLoading { get { return isLoading; } set { isLoading = value; NotifyPropertyChanged("IsLoading"); } }
        private bool isLoading = false;

        public LocalizedResources LocalizedResources { get { return LocalizedResources.Instance; } }

#if WINDOWS_PHONE_APP
        public bool IsMobile { get { return true; } }
        public string OSVersion { get { return "8.1"; } }
#elif WINDOWS_UWP
        public bool IsMobile { get; } = ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1);
        public string OSVersion { get { return "10"; } }
#endif
        public bool IsDesktop { get { return !IsMobile; } }

        public void CloseNote()
        {
            CloseNoteEvent?.Invoke(this, EventArgs.Empty);
        }

        public void OpenNotesPage()
        {
            if (App.RootFrame.CanGoBack)
            {
                App.RootFrame.GoBack();
                return;
            }

            CloseNote();
            App.RootFrame.Navigate(typeof(MainPage));
        }

        public void OpenArchivedNotesPage()
        {
            CloseNote();
            App.RootFrame.Navigate(typeof(ArchivedNotesPage));
        }

        public void OpenSettingsPage()
        {
            CloseNote();
            App.RootFrame.Navigate(typeof(SettingsPage));
        }
    }
}
