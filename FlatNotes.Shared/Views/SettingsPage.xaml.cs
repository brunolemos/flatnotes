using FlatNotes.Common;
using FlatNotes.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FlatNotes.Views
{
    public sealed partial class SettingsPage : UserControl
    {
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register("ShowCloseButton", typeof(bool), typeof(SettingsPage), new PropertyMetadata(false));
        public bool ShowCloseButton { get { return (bool)GetValue(ShowCloseButtonProperty); } set { SetValue(ShowCloseButtonProperty, (value as bool?) == true); } }

        public SettingsViewModel viewModel { get { return _viewModel; } }
        private static SettingsViewModel _viewModel = SettingsViewModel.Instance;

        public SettingsPage()
        {
            this.InitializeComponent();
            Loaded += (s, e) => App.ResetStatusBar();
        }

#if WINDOWS_PHONE_APP
        private void DeveloperTwitterHyperlink_OnClick(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            viewModel.DeveloperTwitterHyperlink_OnClick();
        }
#endif
    }
}
