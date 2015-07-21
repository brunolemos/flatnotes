using System;

namespace FlatNotes.Events
{
    public class NoteIdEventArgs : EventArgs
    {
        public string NoteId { get; private set; }
        public bool Handled { get; set; }

        public NoteIdEventArgs(string id)
        {
            NoteId = id;
            Handled = false;
        }
    }
}