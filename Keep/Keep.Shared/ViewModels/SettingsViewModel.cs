using System.Linq;
using Keep.Utils;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Resources;
using System;
using System.Diagnostics;
using Keep.Common;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.System;

namespace Keep.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public RelayCommand DownloadAppCommand { get; private set; }
        public RelayCommand OpenDeveloperTwitterCommand { get; private set; }
        public RelayCommand SendFeedbackCommand { get; private set; }
        public RelayCommand SuggestFeatureOrReportBugCommand { get; private set; }

        public static string AppName { get { return App.Name; } }
        public static string AppVersion { get { return App.Version; } }
        public static bool IsBeta { get { return App.IsBeta; } }

        public SettingsViewModel()
        {
            DownloadAppCommand = new RelayCommand(DownloadApp);
            OpenDeveloperTwitterCommand = new RelayCommand(OpenDeveloperTwitter);
            SendFeedbackCommand = new RelayCommand(SendFeedback);
            SuggestFeatureOrReportBugCommand = new RelayCommand(SuggestFeatureOrReportBug);
        }

        public bool IsDarkTheme { get { return AppSettings.Instance.Theme == ElementTheme.Dark; } set { AppSettings.Instance.Theme = value ? ElementTheme.Dark : ElementTheme.Light; } }
        public int Columns { get { return AppSettings.Instance.Columns; } set { AppSettings.Instance.Columns = value; } }
        public bool IsSolidColorDefaultTile { get { return !AppSettings.Instance.TransparentTile; } set { AppSettings.Instance.TransparentTile = !value; } }
        public bool IsSolidColorNoteTile { get { return !AppSettings.Instance.TransparentNoteTile; } set { AppSettings.Instance.TransparentNoteTile = !value; } }

        private async void OpenDeveloperTwitter()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "open_developer_twitter", 0);
            await Launcher.LaunchUriAsync(new Uri("http://twitter.com/brunolemos"));
        }

        public static async void DownloadApp()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "download_app", 0);
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:navigate?appid=da5b3964-02d9-42c4-ab1d-5e58d1c06095"));
        }

        public static async void SendFeedback()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "send_feedback", 0);
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }


        public static async void SuggestFeatureOrReportBug()
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("ui_action", "execute_command", "suggest_feature_or_report_bug", App.IsBeta ? 1 : 0);

            string email = "flatnotes@brunolemos.org";
            string subject = String.Format("Feedback - {0} v{1}", App.Name, App.Version);
            string body = ResourceLoader.GetForCurrentView().GetString("YourMessageGoesHere");

#if WINDOWS_PHONE_APP
            Windows.ApplicationModel.Email.EmailMessage mail = new Windows.ApplicationModel.Email.EmailMessage();
            mail.To.Add(new Windows.ApplicationModel.Email.EmailRecipient(email));
            mail.Subject = subject;
            mail.Body = body;
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(mail);
#else
            var emailToUri = new Uri(String.Format("mailto:?to={0}&subject={1}&body={2}", email, subject, body));
            await Windows.System.Launcher.LaunchUriAsync(emailToUri);
#endif
        }
    }
}