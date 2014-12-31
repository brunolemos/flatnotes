using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using Newtonsoft.Json;

using Keep.Utils;
using Keep.Models;
using System.Diagnostics;

namespace Keep.Utils.Migration
{
    public class MigrationV2 : MigrationInterface
    {
        uint Version = 2;

        // The key names of our settings
        public const string NOTESIDS_KEY = "KeepSetting_Notes_IDS";
        public const string NOTE_KEY_FORMAT = "KeepSetting_Note_{0}";

        // The default value of our settings
        public static List<String> NOTESIDS_DEFAULT = new List<String>();

        public async void Up()
        {
            await ApplicationData.Current.SetVersionAsync(Version, appDataVersionHandler);
        }

        private async void appDataVersionHandler(SetVersionRequest request)
        {
            var defer = request.GetDeferral();
            System.Diagnostics.Debug.WriteLine("request handler called with: {0}", request.DesiredVersion);

            //LoggedUser
            string json = (string)ApplicationData.Current.LocalSettings.Values[MigrationV1.LOGGEDUSER_KEY];
            //User LoggedUser = (String.IsNullOrEmpty(json)) ? MigrationV1.LOGGEDUSER_DEFAULT : JsonConvert.DeserializeObject<User>(json);

            StorageFile dataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("data.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(dataFile, json);

            System.Diagnostics.Debug.WriteLine("request handler complete updated {0} items", ApplicationData.Current.LocalSettings.Values.Count);
            Debug.WriteLine("content: " + ApplicationData.Current.LocalSettings.Values[MigrationV1.LOGGEDUSER_KEY]);
            ApplicationData.Current.SignalDataChanged();
            defer.Complete();
        }

        public async void Down()
        {
            StorageFile dataFile = await ApplicationData.Current.LocalFolder.GetFileAsync("data.txt");
            String json = await FileIO.ReadTextAsync(dataFile);
            dataFile.DeleteAsync();

            //Save LoggedUser
            ApplicationData.Current.LocalSettings.Values[MigrationV1.LOGGEDUSER_KEY] = json;
        }

        public uint GetVersion()
        {
            return Version;
        }
    }
}
