using Windows.UI.Xaml.Controls;

namespace FlatNotes.Controls
{
    public partial class MyListView : ListView
    {
#if WINDOWS_APP
        public object ReorderMode { get; set; }
#endif
    }
}