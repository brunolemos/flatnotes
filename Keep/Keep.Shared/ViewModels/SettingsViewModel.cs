using System;
using System.Collections.Generic;
using System.Text;

using Keep.Commands;
using Keep.Models;
using Keep.Utils;

namespace Keep.ViewModels
{
    public class SettingsViewModel
    {
        public UserPreferences UserPreferences { get { return userPreferences; } }
        private UserPreferences userPreferences = AppSettings.Instance.LoggedUser.Preferences;
    }
}
