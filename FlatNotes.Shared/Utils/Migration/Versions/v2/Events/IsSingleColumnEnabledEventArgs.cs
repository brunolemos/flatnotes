using System;

namespace FlatNotes.Utils.Migration.Versions.v2.Events
{
    public class IsSingleColumnEnabledEventArgs : EventArgs
    {
        public bool IsSingleColumnEnabled { get; private set; }

        public IsSingleColumnEnabledEventArgs(bool isEnabled)
        {
            IsSingleColumnEnabled = isEnabled;
        }
    }
}