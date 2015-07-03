using FlatNotes.Events;
using FlatNotes.Models;
using System;
using System.ComponentModel;
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

                var handler = NoteColorChanged;
                if (handler != null && selectedNoteColor != null && selectedNoteColor != value)
                {
                    handler(this, new NoteColorEventArgs(value));
                }

                selectedNoteColor = Colors.Find(nc => nc.Key == value.Key);
                NotifyPropertyChanged("SelectedNoteColor");
            }
        }
        private NoteColor selectedNoteColor = null;

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
        }

        void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
