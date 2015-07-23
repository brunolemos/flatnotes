using System;

namespace FlatNotes.Utils.Migration.Versions.v2.Events
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