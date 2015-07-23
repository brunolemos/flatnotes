using System;

namespace FlatNotes.Utils.Migration.Versions.v2.Events
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