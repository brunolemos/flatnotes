using FlatNotes.Models;
using FlatNotes.Utils;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FlatNotes.Controls
{
    public class NoteColorPickerItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate SelectedItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            //if (DesignMode.DesignModeEnabled) return ItemTemplate;

            //NoteColor noteColor = item as NoteColor;
            //ListViewItem listViewItem = container as ListViewItem;

            //if (noteColor == null || listViewItem == null)
            //    return ItemTemplate;

            //return listViewItem.IsSelected ? SelectedItemTemplate : ItemTemplate;
            return ItemTemplate;
        }
    }
}