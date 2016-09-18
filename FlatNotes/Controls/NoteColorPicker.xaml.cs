using FlatNotes.Common;
using FlatNotes.Events;
using FlatNotes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.Controls
{
    public sealed partial class NoteColorPicker : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<NoteColorEventArgs> NoteColorChanged;

        public NoteColor SelectedNoteColor
        {
            get { return selectedNoteColor; }
            set
            {
                if (value == null || !(value is NoteColor)) value = NoteColor.DEFAULT;
                if (value == selectedNoteColor) return;

                var handler = NoteColorChanged;
                if (handler != null && selectedNoteColor != null && selectedNoteColor != value)
                {
                    handler(this, new NoteColorEventArgs(value));
                }

                selectedNoteColor = Colors.Find(nc => nc.Item.Key == value.Key).Item;
                NotifyPropertyChanged("SelectedNoteColor");

                updateSelectedColor();
            }
        }
        private NoteColor selectedNoteColor = null;

        public List<SelectableItem<NoteColor>> Colors { get { return colors; } private set { colors = value; } }
        private List<SelectableItem<NoteColor>> colors = new List<SelectableItem<NoteColor>>();

        public NoteColorPicker()
        {
            this.InitializeComponent();

            for (var i = 0; i < NoteColor.Colors.Count; i++)
            {
                colors.Add(new SelectableItem<NoteColor>(NoteColor.Colors[i]));
            }
        }

        void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ColorPickerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null || listView.SelectedIndex < 0 || listView.SelectedIndex + 1 > Colors.Count) return;

            SelectedNoteColor = Colors[listView.SelectedIndex].Item;
        }

        private void updateSelectedColor()
        {
            var total = Colors.Count;
            for (var i = 0; i < total; i++)
            {
                Colors[i].IsSelected = SelectedNoteColor == Colors[i].Item;
            }
        }

        private void ColorPickerListView_Loaded(object sender, RoutedEventArgs e)
        {
            var index = Colors.FindIndex(color => color.IsSelected);
            if (index >= 0 && index < ColorPickerListView.Items.Count)
            {
                ColorPickerListView.SelectedIndex = index;
            }
        }
    }
}
