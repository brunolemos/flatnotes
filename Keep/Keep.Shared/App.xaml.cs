using Keep.Common;
using Keep.Utils;
using Keep.ViewModels;
using Keep.Views;
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

namespace Keep
{
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
        public static ContinuationManager ContinuationManager { get; private set; }
#endif

        public static bool IsBeta = Package.Current.Id.Name.Contains("Beta");
        public static string Name = IsBeta ? "Flat Notes Beta" : "Flat Notes";
        public static string Version = String.Format("{0}.{1}.{2}.{3}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
        public static string PublishedMainAppId = "da5b3964-02d9-42c4-ab1d-5e58d1c06095";

        public static Frame RootFrame { get { if (rootFrame == null) rootFrame = CreateRootFrame(); return rootFrame; } }
        private static Frame rootFrame = null;

        public App()
        {
            this.InitializeComponent();

            this.Suspending += this.OnSuspending;
            this.UnhandledException += App_UnhandledException;

            AppSettings.Instance.NotesSaved += (s, e) => SimulateStatusBarProgressComplete();
            AppSettings.Instance.ArchivedNotesSaved += (s, e) => SimulateStatusBarProgressComplete();
            AppSettings.Instance.ThemeChanged += (s, e) => UpdateTheme(e.Theme);
            AppSettings.Instance.TransparentTileChanged += (s, e) => TileManager.UpdateDefaultTile(e.TransparentTile);
            AppSettings.Instance.TransparentNoteTileChanged += (s, e) => TileManager.UpdateAllNoteTilesBackgroundColor(e.TransparentTile);

            AppData.NoteArchived += (s, _e) => { TileManager.RemoveTileIfExists(_e.Note); };
            AppData.NoteRemoved += (s, _e) => { TileManager.RemoveTileIfExists(_e.Note); };

            //ParseClient.Initialize("l3HEDWzlj1zLmkL8l2KH8lBToeVVpUiurHNi8AHv", "w1s6IQUJHUxRQvKIZVQbRgfwJ2PfUz0HLRkhya2K");
        }

        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendException(String.Format("{0} (Stack trace: {1})", e.Message, e.Exception.StackTrace), true);

            if (!e.Handled)
            {
                e.Handled = true;
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

#if WINDOWS_PHONE_APP
            ContinuationManager = new ContinuationManager();

            var continuationEventArgs = e as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null)
            {
                // Call ContinuationManager to handle continuation activation
                if (RootFrame != null)
                    ContinuationManager.Continue(continuationEventArgs, RootFrame);
            }
#endif

            Window.Current.Activate();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
                this.DebugSettings.EnableFrameRateCounter = true;
#endif

            //user theme
            UpdateTheme(AppSettings.Instance.Theme);

            //user preferences
            GoogleAnalytics.EasyTracker.GetTracker().SetCustomDimension(1, AppSettings.Instance.Theme.ToString());
            GoogleAnalytics.EasyTracker.GetTracker().SetCustomDimension(2, AppSettings.Instance.Columns.ToString());

            if (RootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (RootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in RootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                RootFrame.ContentTransitions = null;
                RootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                //load app data
                await AppData.Load();

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

#if WINDOWS_PHONE_APP
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = null;// this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();

            //update tile
            if (NoteEditViewModel.CurrentNoteBeingEdited != null) TileManager.UpdateNoteTileIfExists(NoteEditViewModel.CurrentNoteBeingEdited, AppSettings.Instance.TransparentNoteTile);

            //save data
            if (AppData.HasUnsavedChangesOnNotes) await AppData.SaveNotes();
            if (AppData.HasUnsavedChangesOnArchivedNotes) await AppData.SaveArchivedNotes();

            deferral.Complete();
        }

        private async void UpdateTheme(ElementTheme theme)
        {
            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                RootFrame.RequestedTheme = theme;
                ChangeStatusBarColor();
            });
        }
        
        public static void ChangeStatusBarColor(Color? foregroundColor = null)
        {
#if WINDOWS_PHONE_APP
            if (foregroundColor == null)
            {
                if (AppSettings.Instance.Theme == ElementTheme.Light && App.Current.RequestedTheme == ApplicationTheme.Dark)
                    foregroundColor = new Color().FromHex("#404040");
                else if (AppSettings.Instance.Theme != ElementTheme.Light && App.Current.RequestedTheme == ApplicationTheme.Light)
                    foregroundColor = new Color().FromHex("#c9cdd1");
            }

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