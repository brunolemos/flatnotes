using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

using Keep.Models;
using Windows.UI.Xaml;
using System.Diagnostics;

namespace Keep.Controls
{
    public sealed partial class NoteColorPicker : UserControl
    {
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        public event EventHandler Opened;
        public event EventHandler Closed;

        public NoteColor SelectedNoteColor
        {
            get { return selectedNoteColor; }
            set
            {
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

        public NoteColors Colors { get { return colors; } }
        private NoteColors colors = new NoteColors()
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

        public NoteColorPicker()
        {
            this.InitializeComponent();

            this.Loaded += NoteColorPicker_Loaded;
            ColorPickerVisualStateGroup.CurrentStateChanged += ColorPickerVisualStateGroup_CurrentStateChanged;
        }

        private void NoteColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            Close(false);
        }

        private void ColorPickerVisualStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            var handler = (e.NewState.Name == OpenedVisualState.Name) ? Opened : Closed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void ColorPickerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;

            if (listView.SelectedItem == null || !(listView.SelectedItem is NoteColor))
                listView.SelectedItem = (listView.ItemsSource as IList<NoteColor>)[0];

            SelectedNoteColor = listView.SelectedItem as NoteColor;
        }

        private void OutsideGrid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Close();
        }

        public void Open(bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, OpenedVisualState.Name, useTransitions);
        }

        public void Close(bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, ClosedVisualState.Name, useTransitions);
        }
    }
}
