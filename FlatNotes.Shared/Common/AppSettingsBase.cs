using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;

namespace FlatNotes.Common
{
    public abstract class AppSettingsBase
    {
        protected ApplicationDataContainer localSettings { get { return ApplicationData.Current.LocalSettings; } }
        protected ApplicationDataContainer roamingSettings { get { return ApplicationData.Current.RoamingSettings; } }
        protected StorageFolder localFolder { get { return ApplicationData.Current.LocalFolder; } }

        public abstract uint Version { get; }

        protected AppSettingsBase() { }

        public void ClearLocalSettings()
        {
            localSettings.Values.Clear();
        }

        protected T GetValueOrDefault<T>(string key, T defaultValue, bool useRoaming = false)
        {
            try
            {
                var settings = useRoaming ? roamingSettings : localSettings;
                var value = settings.Values[key];
                if (value == null || String.IsNullOrEmpty(value.ToString())) return defaultValue;
                //Debug.WriteLine("Value of {0} is {1}", key, value.ToString());

                return value.GetType() != typeof(string) ? (T)value : JsonConvert.DeserializeObject<T>(value.ToString());
            }
            catch (Exception e)
            {
                App.TelemetryClient.TrackException(e);
                return defaultValue;
            }
        }

        protected bool SetValue<T>(string key, T value, bool useRoaming = false)
        {
            try
            {
                var settings = useRoaming ? roamingSettings : localSettings;
                string content = JsonConvert.SerializeObject(value);
                settings.Values.Remove(key);
                settings.Values[key] = content;
                //Debug.WriteLine("SetValue of {0} to {1}", key, content);

                return true;
            }
            catch (Exception e)
            {
                App.TelemetryClient.TrackException(e);
                return false;
            }
        }

        protected async Task<T> ReadFileOrDefault<T>(string fileName, T defaultValue)
        {
            try
            {
                StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                string json = await FileIO.ReadTextAsync(file);
                Debug.WriteLine("Content of {0} is {1}", fileName, json);

                if (String.IsNullOrEmpty(json)) return defaultValue;
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                App.TelemetryClient.TrackException(e);
                return defaultValue;
            }
        }

        protected async Task<bool> SaveFile<T>(string fileName, T value)
        {
            try
            {
                StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                string json = JsonConvert.SerializeObject(value);
                Debug.WriteLine("SaveFile {0} with {1}", fileName, json);

                await FileIO.WriteTextAsync(file, json);
                return true;
            }
            catch (Exception e)
            {
                App.TelemetryClient.TrackException(e);
                return false;
            }
        }
    }
}