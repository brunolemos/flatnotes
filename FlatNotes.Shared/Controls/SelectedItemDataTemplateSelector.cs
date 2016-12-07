using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.Controls
{
    public class SelectedItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultItemTemplate { get; set; }
        public DataTemplate SelectedItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var listViewItem = container as ListViewItem;
            //System.Diagnostics.Debug.WriteLine("listViewItem", listViewItem.IsSelected);
            return listViewItem != null && listViewItem.IsSelected ? SelectedItemTemplate : DefaultItemTemplate;
        }
    }
}
