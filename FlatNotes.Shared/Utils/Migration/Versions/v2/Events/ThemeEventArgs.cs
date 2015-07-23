using System;
using Windows.UI.Xaml;

namespace FlatNotes.Utils.Migration.Versions.v2.Events
{
    public class ThemeEventArgs : EventArgs
    {
        public ElementTheme Theme { get; private set; }

        public ThemeEventArgs(ElementTheme theme)
        {
            Theme = theme;
        }
    }
}