using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Keep.ViewModels;
using Keep.Common;
using System.Diagnostics;
using Keep.Utils;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Keep
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private SettingsViewModel viewModel = new SettingsViewModel();
        ElementTheme theme = AppSettings.Instance.LoggedUser.Preferences.Theme;
        Color? statusBarForegroundColor = null;
        SolidColorBrush defaultTextBoxFocusedBackgroundThemeBrush = null;

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public SettingsPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.DataContext = viewModel;

            ThemeComboBox.SelectedIndex = (AppSettings.Instance.LoggedUser.Preferences.Theme == ElementTheme.Light) ? 0 : 1;
            defaultTextBoxFocusedBackgroundThemeBrush = App.Current.Resources["TextBoxFocusedBackgroundThemeBrush"] as SolidColorBrush;
            App.Current.Resources["TextBoxFocusedBackgroundThemeBrush"] = new SolidColorBrush(Colors.White);
            Debug.WriteLine("defaultTextBoxFocusedBackgroundThemeBrush: " + defaultTextBoxFocusedBackgroundThemeBrush.Color.ToString());

#if WINDOWS_PHONE_APP
            StatusBar statusBar = StatusBar.GetForCurrentView();
            statusBarForegroundColor = statusBar.ForegroundColor;
            statusBar.ForegroundColor = ((SolidColorBrush)App.Current.Resources["KeepStatusBarForegroundBrush"]).Color;
#endif
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            //apply theme and save to user preferences
            if(theme != AppSettings.Instance.LoggedUser.Preferences.Theme) App.ForceTheme(theme);
            App.Current.Resources["TextBoxFocusedBackgroundThemeBrush"] = defaultTextBoxFocusedBackgroundThemeBrush;

#if WINDOWS_PHONE_APP
            StatusBar statusBar = StatusBar.GetForCurrentView();
            statusBar.ForegroundColor = statusBarForegroundColor;
#endif
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((e.AddedItems[0] as ComboBoxItem).Tag.ToString())
            {
                case "light":
                    theme = ElementTheme.Light;
                    break;

                case "dark":
                    theme = ElementTheme.Dark;
                    break;
            }

            //apply theme and save to user preferences
            App.ForceTheme(theme);

            //fix
            ThemeComboBox.BorderBrush = this.Foreground;
            ThemeComboBox.Foreground = this.Foreground;
        }
    }
}
