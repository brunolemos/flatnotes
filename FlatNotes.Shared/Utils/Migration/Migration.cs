using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace FlatNotes.Utils.Migration
{
    public static class Migration
    {
        static Action[] UpList = new Action[4]
        {
            null,
            Versions.v1.Utils.AppSettings.Up,
            Versions.v2.Utils.AppSettings.Up,
            AppSettings.Up
        };

        static Action[] DownList = new Action[4]
        {
            null,
            Versions.v1.Utils.AppSettings.Down,
            Versions.v2.Utils.AppSettings.Down,
            AppSettings.Down
        };

        public static async Task Migrate(uint desiredVersion)
        {
            //versioning
            await ApplicationData.Current.SetVersionAsync(desiredVersion, (setVersionRequest) =>
                {
                    int currentVersion = (int)setVersionRequest.CurrentVersion;
                    
                    //disabled because nobody is in the first version anymore -- irrelevant
                    ////fix current version (because v1 was not especified before)
                    //if (currentVersion == 0 && Versions.v1.Utils.AppSettings.Instance.LoggedUser.Notes.Count > 0)
                    //    currentVersion = 1;

                    //no data to migrate or already in the desired version, do nothing
                    if (currentVersion <= 0 || setVersionRequest.DesiredVersion == currentVersion)
                        return;

                    //execute migrations (UPs)
                    if (currentVersion < setVersionRequest.DesiredVersion)
                        for (int i = currentVersion + 1; i <= setVersionRequest.DesiredVersion && i < UpList.Length; i++)
                            if (UpList[i] != null) UpList[i]();

                    //execute migrations (DOWNs)
                    if (currentVersion > setVersionRequest.DesiredVersion)
                        for (int i = (int)currentVersion; i > setVersionRequest.DesiredVersion && i < DownList.Length; i--)
                            if (DownList[i] != null) DownList[i]();
                }
            ).AsTask();
        }
    }
}
