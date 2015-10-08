using FlatNotes.Events;
using FlatNotes.Models;
using FlatNotes.ViewModels;
using FlatNotes.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace FlatNotes.Controls
{
    public sealed partial class NotesControl : UserControl
    {
        public event EventHandler<ItemsReorderedEventArgs> ItemsReordered;

        public NotesControlViewModel viewModel { get { return _viewModel; } }
        private static NotesControlViewModel _viewModel = NotesControlViewModel.Instance;

        public event EventHandler<ItemClickEventArgs> ItemClick;
        public const double ITEM_MIN_WIDTH = 150;
        public const double ITEM_MAX_WIDTH = 1024;

#if WINDOWS_PHONE_APP
        private static NoteSwipeFeature noteSwipeFeature = new NoteSwipeFeature();
#endif

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(NotesControl), new PropertyMetadata(-1));
        public object ItemsSource { get { return (object)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(NotesControl), new PropertyMetadata(-1));
        public int Columns { get { return (int)GetValue(ColumnsProperty); } set { SetValue(ColumnsProperty, value); } }

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(NotesControl), new PropertyMetadata(ITEM_MIN_WIDTH));
        public double ItemWidth { get { return (double)GetValue(ItemWidthProperty); } set { SetValue(ItemWidthProperty, value); } }

        public static readonly DependencyProperty MaxItemWidthProperty = DependencyProperty.Register("MaxItemWidth", typeof(double), typeof(NotesControl), new PropertyMetadata(ITEM_MAX_WIDTH));
        public double MaxItemWidth { get { return (double)GetValue(MaxItemWidthProperty); } set { SetValue(MaxItemWidthProperty, value); } }

        public static readonly DependencyProperty ItemStretchProperty = DependencyProperty.Register("ItemStretch", typeof(bool), typeof(NotesControl), new PropertyMetadata(true));
        public bool ItemStretch { get { return (bool)GetValue(ItemStretchProperty); } set { SetValue(ItemStretchProperty, value); } }

        public static readonly DependencyProperty AllowSingleColumnProperty = DependencyProperty.Register("AllowSingleColumn", typeof(bool), typeof(NotesControl), new PropertyMetadata(true));
        public bool AllowSingleColumn { get { return (bool)GetValue(AllowSingleColumnProperty); } set { SetValue(AllowSingleColumnProperty, value); } }

        public static readonly DependencyProperty BiggerItemHeightProperty = DependencyProperty.Register("BiggerItemHeight", typeof(double), typeof(NotesControl), new PropertyMetadata(0));
        public double BiggerItemHeight { get { return (double)GetValue(BiggerItemHeightProperty); } private set { SetValue(BiggerItemHeightProperty, value); } }

        public static readonly DependencyProperty CanReorderItemsProperty = DependencyProperty.Register("CanReorderItems", typeof(bool), typeof(NotesControl), new PropertyMetadata(false));
        public bool CanReorderItems { get { return (bool)GetValue(CanReorderItemsProperty); } set { SetValue(CanReorderItemsProperty, value); } }

        public static readonly DependencyProperty CanDragItemsProperty = DependencyProperty.Register("CanDragItems", typeof(bool), typeof(NotesControl), new PropertyMetadata(false));
        public bool CanDragItems { get { return (bool)GetValue(CanDragItemsProperty); } set { SetValue(CanDragItemsProperty, value); } }

        public static readonly DependencyProperty CanSwipeItemsProperty = DependencyProperty.Register("CanSwipeItems", typeof(bool), typeof(NotesControl), new PropertyMetadata(false));
        public bool CanSwipeItems { get { return (bool)GetValue(CanSwipeItemsProperty); } set { SetValue(CanSwipeItemsProperty, value); } }

        public static readonly DependencyProperty IsNoteFlyoutEnabledProperty = DependencyProperty.Register("IsNoteFlyoutEnabled", typeof(bool), typeof(NotesControl), new PropertyMetadata(true));
        public bool IsNoteFlyoutEnabled { get { return (bool)GetValue(IsNoteFlyoutEnabledProperty); } set { SetValue(IsNoteFlyoutEnabledProperty, value); } }

        public static readonly DependencyProperty ReorderModeProperty = DependencyProperty.Register("ReorderMode", typeof(ListViewReorderMode), typeof(NotesControl), new PropertyMetadata(ListViewReorderMode.Disabled, onReorderModeChanged));

        private static void onReorderModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as NotesControl).viewModel.reorderMode = (ListViewReorderMode)e.NewValue;
        }

        public ListViewReorderMode ReorderMode { get { return (ListViewReorderMode)GetValue(ReorderModeProperty); } set { SetValue(ReorderModeProperty, value); } }

        FlyoutBase openedFlyout = null;

        public NotesControl()
        {
            this.InitializeComponent();

            Loaded += (s, e) => CreateItemsSourceBindingIfNecessary();
            viewModel.ReorderModeEnabled += (s, e) => ReorderMode = ListViewReorderMode.Enabled;
            viewModel.ReorderModeDisabled += (s, e) => ReorderMode = ListViewReorderMode.Disabled;
        }

        private async void CreateItemsSourceBindingIfNecessary()
        {
            var items = ItemsSource as IEnumerable<object>;
            if (items == null || NotesGridView.Items.Count > 0) return;

            foreach (var item in items)
                NotesGridView.Items.Add(item);

            await Task.Delay(1000);
            NotesGridView.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = this, Path = new PropertyPath("ItemsSource"), Mode = BindingMode.OneWay });
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (NotesGridView.ReorderMode == ListViewReorderMode.Enabled)
                return;

            var handler = ItemClick;
            if (handler != null) handler(sender, e);
        }

        private void NotePreview_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            ShowNoteFlyoutIfEnabled(sender as FrameworkElement);
        }

        private void NotePreview_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            ShowNoteFlyoutIfEnabled(sender as FrameworkElement);
        }

        private void ShowNoteFlyoutIfEnabled(FrameworkElement element)
        {
            if (!IsNoteFlyoutEnabled) return;
            if (ReorderMode == ListViewReorderMode.Enabled) return;
            if (element == null) return;

            openedFlyout = Flyout.GetAttachedFlyout(element);
            if (openedFlyout == null) return;

            openedFlyout.Closed += OpenedFlyout_Closed;

            Flyout.ShowAttachedFlyout(element);
        }

        private void OpenedFlyout_Closed(object sender, object e)
        {
            if (openedFlyout != null)
                openedFlyout.Closed -= OpenedFlyout_Closed;

            openedFlyout = null;
        }

#if WINDOWS_UWP
        public void OnChangeColorClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            if (menuItem == null) return;

            var note = menuItem.DataContext as Note;
            if (note == null) return;

            var newColor = menuItem.CommandParameter as NoteColor;
            if (newColor == null) return;

            viewModel.ChangeColor(note, newColor);
        }
#endif

        private void NotesFluidGrid_ItemsReordered(object sender, Events.ItemsReorderedEventArgs e)
        {
#if WINDOWS_UWP
            ReorderMode = ListViewReorderMode.Disabled;
#endif

            var handler = ItemsReordered;
            if (handler != null) handler(sender, e);
        }

        private void NotesGridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if(CanReorderItems)
                ReorderMode = ListViewReorderMode.Enabled;

            if (openedFlyout != null)
                openedFlyout.Hide();
        }

#if WINDOWS_UWP
        private void NotesGridView_DragItemsCompleted(object sender, DragItemsCompletedEventArgs args)
        {
            ReorderMode = ListViewReorderMode.Disabled;
        }
#endif

        private void OnNoteLoaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if (!CanSwipeItems) return;

            FrameworkElement element = NotesGridView.ContainerFromItem((sender as FrameworkElement).DataContext) as FrameworkElement;
            if (element == null) return;

            EnableSwipeForElement(element);
#endif
        }

#if WINDOWS_PHONE_APP
        private void EnableSwipeForElement(FrameworkElement element)
        {
            if (!CanSwipeItems) return;
            if (element == null) return;

            FrameworkElement referenceFrame = NotesGridView;

            if (viewModel.ReorderMode != ListViewReorderMode.Enabled)
                noteSwipeFeature.EnableSwipeFeature(element, referenceFrame);

            noteSwipeFeature.enableSwipeEventHandlers[element] = (s, _e) => { noteSwipeFeature.EnableSwipeFeature(element, referenceFrame); };
            noteSwipeFeature.disableSwipeEventHandlers[element] = (s, _e) => { noteSwipeFeature.DisableSwipeFeature(element); };

            viewModel.ReorderModeDisabled += noteSwipeFeature.enableSwipeEventHandlers[element];
            viewModel.ReorderModeEnabled += noteSwipeFeature.disableSwipeEventHandlers[element];

            element.Unloaded += (s, e) => DisableSwipeForElement(s as FrameworkElement);
        }

        private void DisableSwipeForElement(FrameworkElement element)
        {
            if (!CanSwipeItems) return;
            if (element == null) return;

            if (noteSwipeFeature.enableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeDisabled -= noteSwipeFeature.enableSwipeEventHandlers[element];
            if (noteSwipeFeature.disableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeEnabled -= noteSwipeFeature.disableSwipeEventHandlers[element];

            noteSwipeFeature.enableSwipeEventHandlers.Remove(element);
            noteSwipeFeature.disableSwipeEventHandlers.Remove(element);

            noteSwipeFeature.DisableSwipeFeature(element);
        }
#endif
    }
}
