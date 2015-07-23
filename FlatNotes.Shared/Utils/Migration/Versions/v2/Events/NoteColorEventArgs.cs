using FlatNotes.Models;
using System;

namespace FlatNotes.Utils.Migration.Versions.v2.Events
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