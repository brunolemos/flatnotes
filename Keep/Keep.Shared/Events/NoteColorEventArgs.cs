using Keep.Models;
using System;

namespace Keep.Events
{
    public class NoteColorEventArgs : EventArgs
    {
        public NoteColor NoteColor { get; private set; }
        public bool Handled { get; set; }

        public NoteColorEventArgs(NoteColor noteColor)
        {
            NoteColor = noteColor;
            Handled = false;
        }
    }
}