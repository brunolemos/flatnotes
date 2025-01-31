﻿using FlatNotes.Models;
using System;

namespace FlatNotes.Utils.Migration.Versions.v2.Events
{
    public class NoteEventArgs : EventArgs
    {
        public Note Note { get; private set; }
        public bool Handled { get; set; }

        public NoteEventArgs(Note note)
        {
            Note = note;
            Handled = false;
        }
    }
}