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
using Windows.Foundation;
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

        public static Color MainColor = Color.FromArgb(0xff, 0xff, 0xbb, 0x00);
        public static bool IsBeta = Package.Current.Id.Name.Contains("Beta");
        public static string Name = IsBeta ? "Flat Notes Beta" : "Flat Notes";
        public static string Version = String.Format("{0}.{1}.{2}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build);
        public static bool IsWP81 = MainViewModel.Instance.IsMobile && MainViewModel.Instance.OSVersion == "8.1";
        public static string OSName = IsWP81 ? "Windows Phone 8.1" : string.Format("Windows {0} {1}", MainViewModel.Instance.OSVersion, MainViewModel.Instance.IsMobile ? "Mobile" : "Desktop");
        public static string PublishedMainAppId = IsWP81 ? "da5b3964-02d9-42c4-ab1d-5e58d1c06095" : "9wzdncrdd0xx";

        public static Frame RootFrame { get { if (rootFrame == null) rootFrame = CreateRootFrame(); return rootFrame; } }
        private static Frame rootFrame = null;

        public App()
        {
            this.UnhandledException += App_UnhandledException;
            this.Suspending += this.OnSuspending;

#if DEBUG
            //disable application insights on debug
            TelemetryConfiguration.Active.DisableTelemetry = true;
#else
            //config application insights
            WindowsAppInitializer.InitializeAsync();//WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.UnhandledException);
#endif

            //application insights has always to be initialized
            TelemetryClient = new TelemetryClient();

            AppSettings.Instance.ThemeChanged += (s, e) => { UpdateTheme(e.Theme); App.TelemetryClient.TrackMetric("Theme", AppSettings.Instance.Theme == ElementTheme.Light ? 1 : 2); };
            AppSettings.Instance.TransparentTileChanged += (s, e) => NotificationsManager.UpdateDefaultTile(e.TransparentTile);
            AppSettings.Instance.TransparentNoteTileChanged += (s, e) => NotificationsManager.UpdateAllNoteTilesBackgroundColor(e.TransparentTile);
            
            AppData.NotesSaved += (s, e) => SimulateStatusBarProgressComplete();
            AppData.NoteColorChanged += async (s, _e) => { await NotificationsManager.UpdateNoteTileBackgroundColor(_e.Note, AppSettings.Instance.TransparentNoteTile); };


            this.InitializeComponent();
        }

        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.Handled) return;

            App.TelemetryClient.TrackException(e.Exception);

            e.Handled = true;
            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                await new MessageDialog(e.Message, "Fatal error").ShowAsync();
                App.Current.Exit();
            });
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
                rootFrame.CacheSize = 10;
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

            if (e is IContinuationActivatedEventArgs)
            {
                ContinuationManager = new ContinuationManager();
                ContinuationManager.Continue(e as IContinuationActivatedEventArgs, RootFrame);
            } else if (e.Kind == ActivationKind.ToastNotification)
            {
                RootFrame.Navigate(typeof(MainPage), (e as IToastNotificationActivatedEventArgs).Argument);
            }

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
            Window.Current.Activate();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            //user theme
            UpdateTheme(AppSettings.Instance.Theme);
            //HideStatusBar();

#if WINDOWS_UWP
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 500));
            //ApplicationView.PreferredLaunchViewSize = new Size(380, 620);
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
#endif

            //update default live tile (because I renamed the image, it was showing empty for some users)
            NotificationsManager.UpdateDefaultTile(AppSettings.Instance.TransparentTile);

            if (RootFrame.Content == null)
            {
                RootFrame.ContentTransitions = null;
                RootFrame.Navigated += this.RootFrame_FirstNavigated;

                //await Windows.Storage.ApplicationData.Current.SetVersionAsync(AppSettings.Instance.Version - 1, (req) => { });

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

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
            Window.Current.Activate();

            //System.Diagnostics.Debug.WriteLine("IsMobile={0}", Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1));
        }

        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = null;// new TransitionCollection() { new NavigationThemeTransition() { } };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var note = NoteEditViewModel.CurrentNoteBeingEdited;
            System.Diagnostics.Debug.WriteLine("OnSuspending {0}", note);

            //update tile
            if (note != null)
            {
                NotificationsManager.UpdateNoteTileIfExists(note, AppSettings.Instance.TransparentNoteTile).ConfigureAwait(false);
                
                //save or remove if empty
                if (note.Changed || note.IsEmpty())
                    AppData.CreateOrUpdateNote(note).ConfigureAwait(false);
            }

            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();

            deferral.Complete();
        }

        private async void UpdateTheme(ElementTheme theme)
        {
            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                RootFrame.RequestedTheme = theme;
                //ResetStatusBar();
            });
        }

        public static async void HideStatusBar()
        {
#if WINDOWS_UWP
            bool hasStatusBar = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
#elif WINDOWS_PHONE_APP
            bool hasStatusBar = true;
#endif

            if (hasStatusBar)
                await StatusBar.GetForCurrentView().HideAsync();
        }

        public static void ResetStatusBar()
        {
            //var mainDarkenColor = Color.FromArgb(0xff, 0xf9, 0x9f, 0x00);
            //var statusBarColor = mainColor.Add(Color.FromArgb(0x10, 255, 255, 255));
            ChangeStatusBarColor(MainColor);
        }

        public static void ChangeStatusBarColor(Color backgroundColor, Color? _foregroundColor = null, ElementTheme? theme = ElementTheme.Dark)
        {
            theme = theme ?? AppSettings.Instance.Theme;
            byte blackOrWhiteByte = theme == ElementTheme.Light ? (byte)0x00 : (byte)0xff;

            Color foregroundColor = _foregroundColor ?? Color.FromArgb(0xD0, blackOrWhiteByte, blackOrWhiteByte, blackOrWhiteByte);
            if (foregroundColor.A < 0xff)
            {
                foregroundColor = backgroundColor.Add(foregroundColor);
                foregroundColor.A = 0xff;
            }

            Color backgroundHoverColor = backgroundColor.Add(Color.FromArgb(0x20, blackOrWhiteByte, blackOrWhiteByte, blackOrWhiteByte)); //10%
            Color foregroundHoverColor = foregroundColor;
            foregroundHoverColor.A = 0xff;

            Color backgroundPressedColor = backgroundHoverColor.Add(Color.FromArgb(0x20, blackOrWhiteByte, blackOrWhiteByte, blackOrWhiteByte)); //10%
            Color foregroundPressedColor = foregroundHoverColor;


            bool hasStatusBar = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
            if (hasStatusBar)
            {
                StatusBar.GetForCurrentView().BackgroundOpacity = backgroundColor.A;
                StatusBar.GetForCurrentView().BackgroundColor = backgroundColor;
                StatusBar.GetForCurrentView().ForegroundColor = foregroundColor;
            }

#if WINDOWS_UWP
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            //System.Diagnostics.Debug.WriteLine("StatuBar background={0}, foreground={1}", backgroundColor, foregroundColor);

            titleBar.ForegroundColor = foregroundColor;
            titleBar.ButtonForegroundColor = foregroundColor;
            titleBar.InactiveForegroundColor = foregroundColor;
            titleBar.ButtonInactiveForegroundColor = foregroundColor;
            titleBar.ButtonHoverForegroundColor = foregroundHoverColor;
            titleBar.ButtonPressedForegroundColor = foregroundPressedColor;

            titleBar.BackgroundColor = backgroundColor;
            titleBar.InactiveBackgroundColor = backgroundColor;
            titleBar.ButtonBackgroundColor = backgroundColor;
            titleBar.ButtonInactiveBackgroundColor = backgroundColor;
            titleBar.ButtonHoverBackgroundColor = backgroundHoverColor;
            titleBar.ButtonPressedBackgroundColor = backgroundPressedColor;
#endif
        }

        public static async void SimulateStatusBarProgressComplete()
        {
#if WINDOWS_UWP
            bool hasStatusBar = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
            if (!hasStatusBar) return;
#endif

            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = 0;
            await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();

            for (int i = 0; i <= 100; i += 10)
            {
                await Task.Delay(0050);
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = i;
            }

            await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
        }
    }
}