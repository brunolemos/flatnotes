using System;
using System.Diagnostics;
using System.Collections.Generic;
using Windows.Storage;

using Newtonsoft.Json;
using System.Threading.Tasks;

using Keep.Models;
using Keep.Utils.Migration;

namespace Keep.Utils
{
    public class AppSettings
    {
        public static readonly AppSettings Instance = new AppSettings();
        private ApplicationDataContainer localSettings;
        private StorageFolder localFolder;

        // The key names of our settings
        private const string LOGGEDUSER_KEY = "KeepSetting_LoggedUser";

        // The default value of our settings
        private User LOGGEDUSER_DEFAULT = new User();

        private AppSettings()
        {
            // Get the settings for this application.
            if (!App.IsDesignMode)
            {
                localSettings = ApplicationData.Current.LocalSettings;
                localFolder = ApplicationData.Current.LocalFolder;
            }

            // Structure version
            Debug.WriteLine("Version: " + ApplicationData.Current.Version.ToString());
            //Keep.Utils.Migration.Migration.Migrate(ApplicationData.Current.Version);

            //String json = GetValueOrDefault<string>(LOGGEDUSER_KEY, "");
            //LoggedUser = (String.IsNullOrEmpty(json)) ? LOGGEDUSER_DEFAULT : JsonConvert.DeserializeObject<User>(json);

            LoggedUser = LOGGEDUSER_DEFAULT;
            //loadData();
        }

        private async void loadData()
        {
            //LoggedUser
            StorageFile dataFile = await localFolder.GetFileAsync("data.txt");
            String json = await FileIO.ReadTextAsync(dataFile);
            Debug.WriteLine("Read from data.txt: " + json);
            LoggedUser = (String.IsNullOrEmpty(json)) ? LOGGEDUSER_DEFAULT : JsonConvert.DeserializeObject<User>(json);
            LoggedUser.NotifyChanges();
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

        public async void SaveLoggedUser()
        {
            await AddOrUpdateValue(LOGGEDUSER_KEY, (LoggedUser == null ? "{}" : JsonConvert.SerializeObject(LoggedUser)));

            //StorageFile dataFile = await localFolder.CreateFileAsync("data.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            //await FileIO.WriteTextAsync(dataFile, JsonConvert.SerializeObject(LoggedUser));
        }
    }
}