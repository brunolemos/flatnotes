using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace Keep.Utils.Migration
{
    public static class Migration
    {
        static List<Action> UpList = new List<Action>()
        {
            null,
            Versions.v1.AppSettings.Instance.Up,
            AppSettings.Instance.Up
        };

        static List<Action> DownList = new List<Action>()
        {
            null,
            Versions.v1.AppSettings.Instance.Down,
            AppSettings.Instance.Down
        };

        public static async Task Migrate(uint desiredVersion)
        {
            //versioning
            await ApplicationData.Current.SetVersionAsync(desiredVersion, (setVersionRequest) =>
                {
                    //fix current version (because v1 was not especified before)
                    uint currentVersion = setVersionRequest.CurrentVersion;
                    if (currentVersion == 0 && Versions.v1.AppSettings.Instance.LoggedUser != Versions.v1.AppSettings.Instance.LOGGEDUSER_DEFAULT)
                        currentVersion = 1;

                    //no data to migrate or already in the desired version, do nothing
                    if (currentVersion <= 0 || setVersionRequest.DesiredVersion == currentVersion)
                        return;

                    //execute migrations (UPs)
                    if (currentVersion < setVersionRequest.DesiredVersion)
                        for (int i = (int)currentVersion; i <= setVersionRequest.DesiredVersion; i++)
                            if (UpList[i] != null) UpList[i]();

                    //execute migrations (DOWNs)
                    if (currentVersion > setVersionRequest.DesiredVersion)
                        for (int i = (int)setVersionRequest.DesiredVersion; i >= currentVersion; i--)
                            if (DownList[i] != null) DownList[i]();
                }
            );
        }
    }
}
