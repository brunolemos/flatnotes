using Windows.Storage;
using Windows.ApplicationModel;
using System;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Keep.Common
{
    public abstract class AppSettingsBase
    {
        protected ApplicationDataContainer localSettings;
        protected StorageFolder localFolder;

        protected AppSettingsBase()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                localSettings = ApplicationData.Current.LocalSettings;
                localFolder = ApplicationData.Current.LocalFolder;
            }
        }

        protected T GetValueOrDefault<T>(string key, T defaultValue)
        {
            try
            {
                var value = localSettings.Values[key];
                if (value == null || String.IsNullOrEmpty(value.ToString())) value = defaultValue;
                //Debug.WriteLine("Value of {0} is {1}", key, value.ToString());

                if (typeof(T) != typeof(string)) return JsonConvert.DeserializeObject<T>(value.ToString());
                return (T)value;
            }
            catch (System.Exception)
            {
                return defaultValue;
            }
        }

        protected bool SetValue<T>(string key, T value)
        {
            try
            {
                string content = typeof(T) == typeof(string) ? value.ToString() :  JsonConvert.SerializeObject(value);
                localSettings.Values[key] = content;
                //Debug.WriteLine("SetValue of {0} to {1}", key, content);

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        protected async Task<T> ReadFileOrDefault<T>(string fileName, T defaultValue)
        {
            try
            {
                return await Task.Run<T>(async () => {
                    StorageFile file = await localFolder.GetFileAsync(fileName);
                    string json = await FileIO.ReadTextAsync(file);
                    Debug.WriteLine("Content of {0} is {1}", fileName, json);

                    if (String.IsNullOrEmpty(json)) return defaultValue;
                    return JsonConvert.DeserializeObject<T>(json);
                });
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
                return false;
            }
        }
    }
}