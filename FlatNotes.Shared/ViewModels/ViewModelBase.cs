using FlatNotes.Common;
using FlatNotes.Utils;
using Windows.Foundation.Metadata;

namespace FlatNotes.ViewModels
{
    public class ViewModelBase : Notifiable
    {
        public bool IsLoaded { get { return isLoaded; } set { isLoaded = value; NotifyPropertyChanged("IsLoaded"); } }
        private bool isLoaded = false;

        public bool IsLoading { get { return isLoading; } set { isLoading = value; NotifyPropertyChanged("IsLoading"); } }
        private bool isLoading = false;

        public LocalizedResources LocalizedResources { get { return LocalizedResources.Instance; } }

#if WINDOWS_PHONE_APP
        public bool IsMobile { get { return true; } }
#elif WINDOWS_UWP
        public bool IsMobile { get; } = ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1);
#endif
    }
}
