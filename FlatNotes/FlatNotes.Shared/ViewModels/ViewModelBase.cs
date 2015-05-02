using FlatNotes.Common;
using FlatNotes.Utils;

namespace FlatNotes.ViewModels
{
    public class ViewModelBase : Notifiable
    {
#if WINDOWS_PHONE_APP
        public bool IsWindows { get { return false; } }
        public bool IsWindowsPhone { get { return true; } }
#else
        public bool IsWindows { get { return true; } }
        public bool IsWindowsPhone { get { return false; } }
#endif

        public LocalizedResources LocalizedResources { get { return LocalizedResources.Instance; } }
    }
}
