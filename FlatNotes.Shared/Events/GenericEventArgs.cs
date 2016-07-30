using System;

namespace FlatNotes.Events
{
    public class GenericEventArgs : EventArgs
    {
        public object Parameter { get; private set; }
        public bool Handled { get; set; }

        public GenericEventArgs(object parameter)
        {
            Parameter = parameter;
            Handled = false;
        }
    }
}