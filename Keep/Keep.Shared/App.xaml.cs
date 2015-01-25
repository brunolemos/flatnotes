using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using Keep.Utils;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Keep.Common;

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
            //different from actual theme
            if (!((RequestedTheme == ApplicationTheme.Light && AppSettings.Instance.Theme == ElementTheme.Light) || (RequestedTheme == ApplicationTheme.Dark && (AppSettings.Instance.Theme == ElementTheme.Dark || AppSettings.Instance.Theme == ElementTheme.Default))))
                RequestedTheme = AppSettings.Instance.Theme == ElementTheme.Light ? ApplicationTheme.Light : ApplicationTheme.Dark;

            this.InitializeComponent();

            AppSettings.Instance.ThemeChanged += (s, e) => UpdateTheme(e.Theme);
            this.Suspending += this.OnSuspending;
            this.UnhandledException += App_UnhandledException;

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

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            //user theme
            //UpdateTheme(AppSettings.Instance.Theme);

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
                if (!RootFrame.Navigate(typeof(Views.MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            //user preferences
            GoogleAnalytics.EasyTracker.GetTracker().SetCustomDimension(1, AppSettings.Instance.Theme.ToString());
            GoogleAnalytics.EasyTracker.GetTracker().SetCustomDimension(2, AppSettings.Instance.Columns.ToString());


            //wait so the splash screen background image may be loaded
            await Task.Delay(0400);

            // Ensure the current window is active
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
            deferral.Complete();
        }

        private async void UpdateTheme(ElementTheme theme)
        {
            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                RootFrame.RequestedTheme = theme;
                //ChangeStatusBarColor();
            });
        }

        //public static void ChangeStatusBarColor(Color? foregroundColor = null)
        //{
//#if WINDOWS_PHONE_APP
//            if (foregroundColor == null) foregroundColor = (App.Current.Resources["AppStatusBarForegroundBrush"] as SolidColorBrush).Color;
//            StatusBar.GetForCurrentView().ForegroundColor = foregroundColor;
//#endif
        //}
    }
}