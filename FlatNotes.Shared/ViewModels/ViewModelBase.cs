using FlatNotes.Common;
using FlatNotes.Utils;
using Windows.Foundation.Metadata;

namespace FlatNotes.ViewModels
{
    public class ViewModelBase : Notifiable
    {
#if WINDOWS_PHONE_APP
        public bool IsMobile { get { return true; } }
#elif WINDOWS_UAP
        public bool IsMobile { get; } = ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1);
#endif

        public LocalizedResources LocalizedResources { get { return LocalizedResources.Instance; } }
    }
}
