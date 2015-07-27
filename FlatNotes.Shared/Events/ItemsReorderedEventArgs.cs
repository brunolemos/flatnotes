using System;

namespace FlatNotes.Events
{
    public class ItemsReorderedEventArgs : EventArgs
    {
        public int OldItemIndex { get; private set; }
        public int NewItemIndex { get; private set; }
        public bool Handled { get; set; } = false;

        public ItemsReorderedEventArgs(int oldItemIndex, int newItemIndex)
        {
            OldItemIndex = oldItemIndex;
            NewItemIndex = newItemIndex;
        }
    }
}