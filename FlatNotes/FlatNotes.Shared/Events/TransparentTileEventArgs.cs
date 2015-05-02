using System;

namespace FlatNotes.Events
{
    public class TransparentTileEventArgs : EventArgs
    {
        public bool TransparentTile { get; private set; }

        public TransparentTileEventArgs(bool transparentTile)
        {
            TransparentTile = transparentTile;
        }
    }
}