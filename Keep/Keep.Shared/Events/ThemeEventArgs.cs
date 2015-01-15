using System;
using Windows.UI.Xaml;

namespace Keep.Events
{
    public class ThemeEventArgs : EventArgs
    {
        public ElementTheme Theme { get; private set; }

        public ThemeEventArgs(ElementTheme theme)
        {
            this.Theme = theme;
        }
    }
}