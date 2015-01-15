﻿using Keep.Models;
using System;

namespace Keep.Events
{
    public class NoteEventArgs : EventArgs
    {
        public Note Note { get; private set; }
        public bool Handled { get; set; }

        public NoteEventArgs(Note note)
        {
            this.Note = note;
            this.Handled = false;
        }
    }
}