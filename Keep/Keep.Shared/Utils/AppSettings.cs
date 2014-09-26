using System;
using System.Diagnostics;
using System.Collections.Generic;
using Windows.Storage;

using Newtonsoft.Json;
using Keep.Models;
using System.Threading.Tasks;

namespace Keep.Utils
{
    public class AppSettings
    {
        public static readonly AppSettings Instance = new AppSettings();
        private ApplicationDataContainer localSettings;

        // The key names of our settings
        private const string LOGGEDUSER_KEY = "KeepSetting_LoggedUser";

        // The default value of our settings
        private User LOGGEDUSER_DEFAULT = new User();

        private AppSettings()
        {
            // Get the settings for this application.
            if (!App.IsDesignMode)
                localSettings = ApplicationData.Current.LocalSettings;

            //LoggedUser
            string json = GetValueOrDefault<string>(LOGGEDUSER_KEY, ""); 
            Debug.WriteLine(json); 
            LoggedUser = (String.IsNullOrEmpty(json)) ? LOGGEDUSER_DEFAULT : JsonConvert.DeserializeObject<User>(json);
        }

        public async Task<bool> AddOrUpdateValue( string Key, Object value )
        {
            Debug.WriteLine("Saving");
            return await Task.Run<bool>(() =>
            {
                bool valueChanged = false;

                // If the key exists
                if (localSettings.Values.ContainsKey(Key))
                {
                    // If the value has changed
                    if (localSettings.Values[Key] != value)
                    {
                        valueChanged = true;

                        // Store the new value
                        localSettings.Values[Key] = value;
                    }

                }
                // Otherwise create the key.
                else
                {
                    localSettings.Values.Add(Key, value);
                    valueChanged = true;
                }

                return valueChanged;
            });
        }

        public T GetValueOrDefault<T>( string Key, T defaultValue )
        {
            // If the key exists, retrieve the value.
            if (localSettings.Values.ContainsKey(Key))
            {
                return (T)localSettings.Values[Key];
            }

            // Otherwise, use the default value.
            return defaultValue;
        }

        public User LoggedUser
        {
            get { return loggedUser; }
            set { loggedUser = value; /*SaveLoggedUser(); loggedUser.PropertyChanged += (s, e) => SaveLoggedUser();*/ }
        }
        private User loggedUser;

        public void SaveLoggedUser()
        {
            AddOrUpdateValue(LOGGEDUSER_KEY, (LoggedUser == null ? "{}" : JsonConvert.SerializeObject(LoggedUser)));
        }
    }
}