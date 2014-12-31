using System;
using System.Collections.Generic;
using System.Text;
using Keep.Models;
using Keep.Utils;
using Windows.Storage;
using System.Diagnostics;

namespace Keep.Utils.Migration
{
   public class MigrationV1 : MigrationInterface
    {
        uint Version = 1;
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        // The key names of our settings
        public const string LOGGEDUSER_KEY = "KeepSetting_LoggedUser";

        // The default value of our settings
        public static User LOGGEDUSER_DEFAULT = new User();

        public async void Up()
        {
            await ApplicationData.Current.SetVersionAsync(Version, appDataVersionHandler);
        }

        private void appDataVersionHandler(SetVersionRequest request)
        {
            Debug.WriteLine("request handler called with: {0}", request.DesiredVersion);
            Debug.WriteLine("request handler complete updated {0} items", ApplicationData.Current.LocalSettings.Values.Count);
            Debug.WriteLine("content: " + ApplicationData.Current.LocalSettings.Values[MigrationV1.LOGGEDUSER_KEY]);
        }

        public void Down()
        {
            //AppSettings.Instance.AddOrUpdateValue("KeepSetting_LoggedUser", null);
        }

        public uint GetVersion()
        {
            return Version;
        }
    }
}
