using Windows.UI.Xaml.Controls;

namespace FlatNotes.Controls
{
    public partial class MyListView : ListView
    {
#if WINDOWS_PHONE_APP
#else
        public object ReorderMode { get; set; }
#endif
    }
}