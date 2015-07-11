using System;

namespace FlatNotes.Events
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