using System;

namespace FlatNotes.Events
{
    public class ColumnsEventArgs : EventArgs
    {
        public int Columns { get; private set; }

        public ColumnsEventArgs(int columns)
        {
            Columns = columns;
        }
    }
}