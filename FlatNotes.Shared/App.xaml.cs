using FlatNotes.Common;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using FlatNotes.Views;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

#if WINDOWS_APP
using Windows.UI.ApplicationSettings;
#endif

namespace FlatNotes
{
    public sealed partial class App : Application
    {
        /// <summary>
        /// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
        /// </summary>
        public static TelemetryClient TelemetryClient;

        public static ContinuationManager ContinuationManager { get; private set; }

        public static bool IsBeta = Package.Current.Id.Name.Contains("Beta");
        public static string Name = IsBeta ? "Flat Notes Beta" : "Flat Notes";
        public static string Version = String.Format("{0}.{1}.{2}.{3}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
        public static string PublishedMainAppId = "da5b3964-02d9-42c4-ab1d-5e58d1c06095";

        public static Frame RootFrame { get { if (rootFrame == null) rootFrame = CreateRootFrame(); return rootFrame; } }
        private static Frame rootFrame = null;

        public App()
        {
#if DEBUG
            //disable application insights on debug
            TelemetryConfiguration.Active.DisableTelemetry = true;
#else
            //config application insights
            WindowsAppInitializer.InitializeAsync(WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.UnhandledException);
#endif

            //application insights has always to be initialized
            TelemetryClient = new TelemetryClient();

            this.InitializeComponent();

            this.Suspending += this.OnSuspending;
            this.UnhandledException += App_UnhandledException;

            AppSettings.Instance.NotesSaved += (s, e) => SimulateStatusBarProgressComplete();
            AppSettings.Instance.ArchivedNotesSaved += (s, e) => SimulateStatusBarProgressComplete();
            AppSettings.Instance.ThemeChanged += (s, e) => { UpdateTheme(e.Theme); App.TelemetryClient.TrackMetric("Theme", AppSettings.Instance.Theme == ElementTheme.Light ? 1 : 2); };
            AppSettings.Instance.TransparentTileChanged += (s, e) => TileManager.UpdateDefaultTile(e.TransparentTile);
            AppSettings.Instance.TransparentNoteTileChanged += (s, e) => TileManager.UpdateAllNoteTilesBackgroundColor(e.TransparentTile);
            
            AppData.NoteArchived += (s, _e) => { TileManager.RemoveTileIfExists(_e.Note.ID); };
            AppData.NoteRemoved += (s, _e) => { TileManager.RemoveTileIfExists(_e.NoteId); };
        }

        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //App.TelemetryClient.TrackException(e.Exception);

            if (!e.Handled)
            {
                //e.Handled = true;
                await new MessageDialog(e.Message, "Fatal error").ShowAsync();

                App.Current.Exit();
            }
        }

        private static Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content, 
            // just ensure that the window is active 
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context
                rootFrame = new Frame();
                rootFrame.CacheSize = 2; //1
            }

            Window.Current.Content = rootFrame;

            return rootFrame;
        }

        private async Task RestoreStatusAsync(ApplicationExecutionState previousExecutionState)
        {
            // Do not repeat app initialization when the Window already has content, 
            // just ensure that the window is active 
            if (previousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Restore the saved session state only when appropriate 
                try
                {
                    await SuspensionManager.RestoreAsync();
                }
                catch (SuspensionManagerException)
                {
                    //Something went wrong restoring state. 
                    //Assume there is no state and continue 
                }
            }
        }
        
        protected override async void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);
            await RestoreStatusAsync(e.PreviousExecutionState);

            ContinuationManager = new ContinuationManager();

            var continuationEventArgs = e as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null)
            {
                // Call ContinuationManager to handle continuation activation
                if (RootFrame != null)
                    ContinuationManager.Continue(continuationEventArgs, RootFrame);
            }

            Window.Current.Activate();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            //user theme
            UpdateTheme(AppSettings.Instance.Theme);

            if (RootFrame.Content == null)
            {
                RootFrame.ContentTransitions = null;
                RootFrame.Navigated += this.RootFrame_FirstNavigated;

                //await Windows.Storage.ApplicationData.Current.SetVersionAsync(AppSettings.Instance.Version-1, (req) => { });

                //prepare app data
                await AppData.Init();
                AppData.LoadNotesIfNecessary();

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!RootFrame.Navigate(typeof(MainPage), e.Arguments))
                    throw new Exception("Failed to create initial page");
            }

            //received parameters and the app was suspended
            else if (e.Arguments != null && !String.IsNullOrEmpty(e.Arguments.ToString()))
            {
                RootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            Window.Current.Activate();
        }

        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = null;// transitions ?? new TransitionCollection() { new EntranceThemeTransition() { FromHorizontalOffset = 50, FromVerticalOffset = 0, IsStaggeringEnabled = false } };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();

            //update tile
            if (NoteEditViewModel.CurrentNoteBeingEdited != null)
                await TileManager.UpdateNoteTileIfExists(NoteEditViewModel.CurrentNoteBeingEdited, AppSettings.Instance.TransparentNoteTile);

            deferral.Complete();
        }

        private async void UpdateTheme(ElementTheme theme)
        {
            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                RootFrame.RequestedTheme = theme;
                ResetStatusBar();
            });
        }

        public static void ResetStatusBar()
        {
            Color mainColor = Color.FromArgb(0xff, 0xff, 0xbb, 0x00);
            //Color mainDarkenColor = Color.FromArgb(0xff, 0xf9, 0x9f, 0x00);
            ChangeStatusBarColor(mainColor);

#if WINDOWS_APP
#else
#if WINDOWS_UAP

            bool hasStatusBar = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
            if (!hasStatusBar) return;
#endif
            //await StatusBar.GetForCurrentView().HideAsync();
#endif
        }

        public static void ChangeStatusBarColor(Color backgroundColor, Color? foregroundColor = null)
        {
            if (foregroundColor == null)
                foregroundColor = Color.FromArgb(0xA0, 0x00, 0x00, 0x00);

#if WINDOWS_APP
#else
#if WINDOWS_UAP
        var titleBar = ApplicationView.GetForCurrentView().TitleBar;

        titleBar.BackgroundColor = backgroundColor;
        titleBar.ForegroundColor = foregroundColor;

        titleBar.ButtonBackgroundColor = backgroundColor;
        titleBar.ButtonForegroundColor = foregroundColor;

        titleBar.ButtonInactiveBackgroundColor = backgroundColor;
        titleBar.ButtonInactiveForegroundColor = Colors.LightGray;

        titleBar.ButtonHoverBackgroundColor = backgroundColor.Add(Color.FromArgb(0x10, 0xff, 0xff, 0xff));
        titleBar.ButtonHoverForegroundColor = titleBar.ButtonForegroundColor;

        titleBar.ButtonPressedBackgroundColor = backgroundColor.Add(Color.FromArgb(0x20, 0xff, 0xff, 0xff));
        titleBar.ButtonPressedForegroundColor = foregroundColor;

        bool hasStatusBar = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
        if (!hasStatusBar) return;
#endif

        //if (foregroundColor == null)
        //{
        //    if (AppSettings.Instance.Theme == ElementTheme.Light && App.Current.RequestedTheme == ApplicationTheme.Dark)
        //        foregroundColor = new Color().FromHex("#404040");
        //    else if (AppSettings.Instance.Theme != ElementTheme.Light && App.Current.RequestedTheme == ApplicationTheme.Light)
        //        foregroundColor = new Color().FromHex("#c9cdd1");
        //}

        StatusBar.GetForCurrentView().BackgroundOpacity = backgroundColor.A;
        StatusBar.GetForCurrentView().BackgroundColor = backgroundColor;
        StatusBar.GetForCurrentView().ForegroundColor = foregroundColor;
#endif
        }

#if WINDOWS_PHONE_APP
        public static async void SimulateStatusBarProgressComplete()
        {
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = 0;
            await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();

            for (int i = 0; i <= 100; i += 10)
            {
                await Task.Delay(0050);
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = i;
            }

            await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
        }
#else
        public static void SimulateStatusBarProgressComplete() { }
#endif

#if WINDOWS_APP
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var preferencesSettings = new SettingsCommand("preferences_settings", "Preferences", (handler) => { new PreferencesSettingsFlyout().Show(); });
            var aboutSettings = new SettingsCommand("about_settings", "About", (handler) => { new AboutSettingsFlyout().Show(); });
            var feedbackSettings = new SettingsCommand("feedback_settings", "Send a nice feedback", (handler) => SettingsViewModel.SendFeedback());
            var reportBugSettings = new SettingsCommand("report_bug_settings", "Report bug / Suggest feature", (handler) => SettingsViewModel.SuggestFeatureOrReportBug());
            var downloadWindowsPhoneSettings = new SettingsCommand("download_wp_settings", "Download app for Windows Phone", (handler) => SettingsViewModel.DownloadWindowsPhoneApp());

            args.Request.ApplicationCommands.Add(preferencesSettings);
            args.Request.ApplicationCommands.Add(aboutSettings);
            args.Request.ApplicationCommands.Add(feedbackSettings);
            args.Request.ApplicationCommands.Add(reportBugSettings);
            args.Request.ApplicationCommands.Add(downloadWindowsPhoneSettings);
        }
#endif

        public static void OpenSettings()
        {
#if WINDOWS_APP
            new PreferencesSettingsFlyout().Show();
#else
            RootFrame.Navigate(typeof(SettingsPage));
#endif
        }
    }
}