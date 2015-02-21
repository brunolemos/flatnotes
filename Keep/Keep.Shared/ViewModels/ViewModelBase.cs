using Keep.Common;

namespace Keep.ViewModels
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
    }
}
