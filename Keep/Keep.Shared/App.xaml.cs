using Keep.Common;
using Keep.Utils;
using Keep.ViewModels;
using Keep.Views;
using System;
using System.Linq;
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

namespace Keep
{
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        public static Frame RootFrame { get { if (rootFrame == null) rootFrame = CreateRootFrame(); return rootFrame; } }
        private static Frame rootFrame = null;

        public static ContinuationManager ContinuationManager { get; private set; }

        public App()
        {
            this.InitializeComponent();

            this.Suspending += this.OnSuspending;
            this.UnhandledException += App_UnhandledException;

            AppSettings.Instance.NotesSaved += (s, e) => SimulateStatusBarProgressComplete();
            AppSettings.Instance.ArchivedNotesSaved += (s, e) => SimulateStatusBarProgressComplete();
            AppSettings.Instance.ThemeChanged += (s, e) => UpdateTheme(e.Theme);
            AppSettings.Instance.TransparentTileChanged += (s, e) => TileManager.UpdateDefaultTile(e.TransparentTile);

            AppData.NoteArchived += (s, _e) => { TileManager.RemoveTileIfExists(_e.Note); };
            AppData.NoteRemoved += (s, _e) => { TileManager.RemoveTileIfExists(_e.Note); };
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
            System.Diagnostics.Debug.WriteLine("OnActivated " + e.PreviousExecutionState.ToString());

            ContinuationManager = new ContinuationManager();

            await RestoreStatusAsync(e.PreviousExecutionState);

            var continuationEventArgs = e as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null)
            {
                // Call ContinuationManager to handle continuation activation
                if (RootFrame != null)
                    ContinuationManager.Continue(continuationEventArgs, RootFrame);
            }

            Window.Current.Activate();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
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

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!RootFrame.Navigate(typeof(SplashPage), e.Arguments))
                    throw new Exception("Failed to create initial page");
            }

            //received parameters and the app was suspended
            else if (e.Arguments != null && !String.IsNullOrEmpty(e.Arguments.ToString()))
            {
                Window.Current.Activate();
                RootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
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
            //if (NoteEditViewModel.CurrentNoteBeingEdited != null) TileManager.UpdateNoteTileIfExists(NoteEditViewModel.CurrentNoteBeingEdited);

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

        public static async void SimulateStatusBarProgressComplete()
        {
#if WINDOWS_PHONE_APP
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = 0;
            await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();

            for (int i = 0; i <= 100; i += 10)
            {
                await Task.Delay(0050);
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = i;
            }

            await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
#endif
        }
    }
}