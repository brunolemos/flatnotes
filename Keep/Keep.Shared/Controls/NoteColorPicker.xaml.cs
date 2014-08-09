using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Keep.Models;

namespace Keep.Controls
{
    public sealed partial class NoteColorPicker : UserControl
    {
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public NoteColor SelectedNoteColor 
        { 
            get { return selectedNoteColor; } 
            set { 
                var handler = SelectionChanged;
                if (handler != null && selectedNoteColor != value)
                {
                    IList<object> removedItems = new List<object>() { selectedNoteColor as object };
                    IList<object> addedItems = new List<object>() { value as object };
                    handler(this, new SelectionChangedEventArgs(removedItems, addedItems));
                }
                
                selectedNoteColor = value; 
            } 
        }
        private NoteColor selectedNoteColor;

        public NoteColorPicker()
        {
            this.InitializeComponent();

            this.DataContext = new NoteColors()
            {
                NoteColor.DEFAULT,
                NoteColor.RED,
                NoteColor.ORANGE,
                NoteColor.YELLOW,
                NoteColor.GRAY,
                NoteColor.BLUE,
                NoteColor.TEAL,
                NoteColor.GREEN
            };
        }

        private void ColorPickerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;

            if (listView.SelectedItem == null || !(listView.SelectedItem is NoteColor))
                listView.SelectedItem = (listView.ItemsSource as IList<NoteColor>)[0];

            SelectedNoteColor = listView.SelectedItem as NoteColor;
        }
    }
}
