using FlatNotes.Common;
using FlatNotes.Utils;
using System;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Xaml;

namespace FlatNotes.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public static SettingsViewModel Instance { get { if (instance == null) instance = new SettingsViewModel(); return instance; } }
        private static SettingsViewModel instance = null;

        public RelayCommand DownloadAppCommand { get; private set; }
        public RelayCommand RateAppCommand { get; private set; }
        public RelayCommand ContactSupportCommand { get; private set; }

        public string AppName { get { return App.Name; } }
        public string AppVersion { get { return App.Version; } }
        public bool IsBeta { get { return App.IsBeta; } }

        private SettingsViewModel()
        {
            DownloadAppCommand = new RelayCommand(DownloadApp);
            RateAppCommand = new RelayCommand(RateApp);
            ContactSupportCommand = new RelayCommand(ContactSupport);
        }

        public bool? IsDarkTheme { get { return AppSettings.Instance.Theme == ElementTheme.Dark; } set { AppSettings.Instance.Theme = value == true ? ElementTheme.Dark : ElementTheme.Light; } }
        public bool? IsSolidColorDefaultTile { get { return !AppSettings.Instance.TransparentTile; } set { AppSettings.Instance.TransparentTile = value != true; } }
        public bool? IsSolidColorNoteTile { get { return !AppSettings.Instance.TransparentNoteTile; } set { AppSettings.Instance.TransparentNoteTile = value != true; } }

        public void DeveloperTwitterHyperlink_OnClick()
        {
            App.TelemetryClient.TrackEvent("OpenDeveloperTwitter_SettingsViewModel");
        }

        public static async void DownloadApp()
        {
            App.TelemetryClient.TrackEvent("DownloadApp_SettingsViewModel");
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:navigate?appid=" + App.PublishedMainAppId));
        }

        public static async void RateApp()
        {
            App.TelemetryClient.TrackEvent("SendFeedback_SettingsViewModel");
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }


        public static async void ContactSupport()
        {
            App.TelemetryClient.TrackEvent("ContactSupport_SettingsViewModel");

            string OSName = MainViewModel.Instance.IsMobile && MainViewModel.Instance.OSVersion == "8.1"
                ? "Windows Phone 8.1"
                : string.Format("Windows {0} {1}", MainViewModel.Instance.OSVersion, MainViewModel.Instance.IsMobile ? "Mobile" : "Desktop");

            string email = "flatnotes@brunolemos.org";
            string subject = string.Format("Feedback - {0} v{1} ({2})", App.Name, App.Version, OSName);
            string body = ResourceLoader.GetForCurrentView().GetString("YourMessageGoesHere");

#if WINDOWS_PHONE_APP
            Windows.ApplicationModel.Email.EmailMessage mail = new Windows.ApplicationModel.Email.EmailMessage();
            mail.To.Add(new Windows.ApplicationModel.Email.EmailRecipient(email));
            mail.Subject = subject;
            mail.Body = body;
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(mail);
#else
            var emailToUri = new Uri(String.Format("mailto:?to={0}&subject={1}&body={2}", email, subject, body));
            await Launcher.LaunchUriAsync(emailToUri);
#endif
        }
    }
}