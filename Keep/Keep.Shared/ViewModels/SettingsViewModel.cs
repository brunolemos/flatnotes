using System;
using System.Collections.Generic;
using System.Text;

using Keep.Commands;
using Keep.Models;
using Keep.Utils;
using Windows.UI.Xaml;

namespace Keep.ViewModels
{
    public class SettingsViewModel
    {
        public UserPreferences Preferences { get { return userPreferences; } set { userPreferences = value; } }
        public UserPreferences UserPreferences { get { return userPreferences; } set { userPreferences = value; } }
        private UserPreferences userPreferences = AppSettings.Instance.LoggedUser.Preferences;
    }
}
