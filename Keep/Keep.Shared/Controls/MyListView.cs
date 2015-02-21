using Windows.UI.Xaml.Controls;

namespace Keep.Controls
{
    public partial class MyListView : ListView
    {
#if WINDOWS_APP
        public object ReorderMode { get; set; }
#endif
    }
}