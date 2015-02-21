using Keep.Events;
using Keep.Models;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Keep.Controls
{
    public sealed partial class NoteColorPicker : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<NoteColorEventArgs> NoteColorChanged;
        public event EventHandler Opened;
        public event EventHandler Closed;

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
        void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ColorsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (ClosedVisualStatePositionAnimation != null)
                ClosedVisualStatePositionAnimation.To = (sender as FrameworkElement).Height;
        }

        private void ColorsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ClosedVisualStatePositionAnimation != null)
                ClosedVisualStatePositionAnimation.To = e.NewSize.Height;
        }
    }
}
