using System;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using Keep.Utils.Migration.Versions.v1.Models;

namespace Keep.Utils.Migration.Versions.v1
{
    public class AppSettings
    {
        public static readonly AppSettings Instance = new AppSettings();
        private ApplicationDataContainer localSettings;

        // The key names of our settings
        private const string LOGGEDUSER_KEY = "KeepSetting_LoggedUser";

        // The default value of our settings
        public User LOGGEDUSER_DEFAULT = new User();

        public void Up()
        {
            return;
        }

        public async void Down()
        {
            await ApplicationData.Current.ClearAsync();
        }

        private AppSettings()
        {
            // Get the settings for this application.
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                localSettings = ApplicationData.Current.LocalSettings;

            String json = GetValueOrDefault<string>(LOGGEDUSER_KEY, "");
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
            //simulation
            //object json = "{\"AnonymousID\":null,\"DevicesList\":[],\"Name\":null,\"Email\":null,\"Notes\":[{\"ID\":\"63557205785323_81016993\",\"IsChecklist\":true,\"Title\":\"C\",\"Text\":\"\",\"Images\":[],\"Checklist\":[{\"Text\":\"C\",\"IsChecked\":false},{\"Text\":\"C\",\"IsChecked\":false},{\"Text\":\"C\",\"IsChecked\":true}],\"CreatedAt\":\"2015-01-18T19:23:05.3232166-02:00\",\"UpdatedAt\":\"2015-01-18T19:23:05.3232166-02:00\",\"Color\":\"GREEN\"},{\"ID\":\"63557205769921_32577953\",\"IsChecklist\":true,\"Title\":\"B\",\"Text\":\"\",\"Images\":[],\"Checklist\":[{\"Text\":\"B\",\"IsChecked\":false},{\"Text\":\"B\",\"IsChecked\":true},{\"Text\":\"B\",\"IsChecked\":false}],\"CreatedAt\":\"2015-01-18T19:22:49.9209317-02:00\",\"UpdatedAt\":\"2015-01-18T19:22:49.9209317-02:00\",\"Color\":\"YELLOW\"}],\"ArchivedNotes\":[{\"ID\":\"63557205760085_92979168\",\"IsChecklist\":false,\"Title\":\"A\",\"Text\":\"A\",\"Images\":[],\"Checklist\":[],\"CreatedAt\":\"2015-01-18T19:22:40.0986598-02:00\",\"UpdatedAt\":\"2015-01-18T19:22:40.0986598-02:00\",\"Color\":\"BLUE\"}],\"Preferences\":{\"Columns\":3,\"ItemMinWidth\":150.0,\"Theme\":0},\"CreatedAt\":\"2015-01-18T19:06:32.6837239-02:00\",\"UpdatedAt\":\"0001-01-01T00:00:00\",\"LastSeenAt\":\"2015-01-18T19:06:32.6867434-02:00\"}";
            //if (Key == LOGGEDUSER_KEY) return (T)json;

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