using System;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using FlatNotes.Utils.Migration.Versions.v1.Models;

namespace FlatNotes.Utils.Migration.Versions.v1.Utils
{
    public class AppSettings
    {
        public static AppSettings Instance { get { if (instance == null) instance = new AppSettings(); return instance; } }
        private static AppSettings instance;

        private ApplicationDataContainer localSettings;

        // The key names of our settings
        private const string LOGGEDUSER_KEY = "KeepSetting_LoggedUser";

        // The default value of our settings
        public User LOGGEDUSER_DEFAULT = new User();

        /// <summary>
        /// Migrate to v1 -- nothing need to be done
        /// </summary>
        public static void Up()
        {
        }

        /// <summary>
        /// Migrate from v1 to nothing -- deletes everything
        /// </summary>
        public static void Down()
        {
            Task.Run(async () =>
            {
                await ApplicationData.Current.ClearAsync();
            }).Wait();
        }

        private AppSettings()
        {
            // Get the settings for this application.
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                localSettings = ApplicationData.Current.LocalSettings;

            String json = GetValueOrDefault<string>(LOGGEDUSER_KEY, "{}");
            LoggedUser = (String.IsNullOrEmpty(json)) ? LOGGEDUSER_DEFAULT : JsonConvert.DeserializeObject<User>(json);
        }


        public async Task<bool> AddOrUpdateValue( string Key, Object value )
        {
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
                return (T)localSettings.Values[Key];

            // Otherwise, use the default value.
            return defaultValue;
        }

        public User LoggedUser
        {
            get { return loggedUser; }
            set { loggedUser = value; /*SaveLoggedUser(); loggedUser.PropertyChanged += (s, e) => SaveLoggedUser();*/ }
        }
        private User loggedUser;

        public async void SaveLoggedUser()
        {
            await AddOrUpdateValue(LOGGEDUSER_KEY, (LoggedUser == null ? "{}" : JsonConvert.SerializeObject(LoggedUser)));
        }
    }
}