using FlatNotes.Events;
using FlatNotes.Models;
using FlatNotes.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
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
        public const double ITEM_MIN_WIDTH = 160;

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(NotesControl), new PropertyMetadata(-1));
        public object ItemsSource { get { return (object)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(NotesControl), new PropertyMetadata(-1));
        public int Columns { get { return (int)GetValue(ColumnsProperty); } set { SetValue(ColumnsProperty, value); } }

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(NotesControl), new PropertyMetadata(ITEM_MIN_WIDTH));
        public double ItemWidth { get { return (double)GetValue(ItemWidthProperty); } set { SetValue(ItemWidthProperty, value); } }

        public static readonly DependencyProperty ItemStretchProperty = DependencyProperty.Register("ItemStretch", typeof(bool), typeof(NotesControl), new PropertyMetadata(false));
        public bool ItemStretch { get { return (bool)GetValue(ItemStretchProperty); } set { SetValue(ItemStretchProperty, value); } }

        public static readonly DependencyProperty AllowSingleColumnProperty = DependencyProperty.Register("AllowSingleColumn", typeof(bool), typeof(NotesControl), new PropertyMetadata(false));
        public bool AllowSingleColumn { get { return (bool)GetValue(AllowSingleColumnProperty); } set { SetValue(AllowSingleColumnProperty, value); } }

        public static readonly DependencyProperty BiggerItemHeightProperty = DependencyProperty.Register("BiggerItemHeight", typeof(double), typeof(NotesControl), new PropertyMetadata(0));
        public double BiggerItemHeight { get { return (double)GetValue(BiggerItemHeightProperty); } private set { SetValue(BiggerItemHeightProperty, value); } }

        public static readonly DependencyProperty CanReorderItemsProperty = DependencyProperty.Register("CanReorderItems", typeof(bool), typeof(NotesControl), new PropertyMetadata(false));
        public bool CanReorderItems { get { return (bool)GetValue(CanReorderItemsProperty); } set { SetValue(CanReorderItemsProperty, value); } }

        public static readonly DependencyProperty CanDragItemsProperty = DependencyProperty.Register("CanDragItems", typeof(bool), typeof(NotesControl), new PropertyMetadata(false));
        public bool CanDragItems { get { return (bool)GetValue(CanDragItemsProperty); } set { SetValue(CanDragItemsProperty, value); } }

        FlyoutBase openedFlyout = null;

        public NotesControl()
        {
            this.InitializeComponent();
            Loaded += (s, e) => CreateItemsSourceBindingIfNecessary();
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
            var handler = ItemClick;
            if (handler != null) handler(sender, e);
        }

        private void NotePreview_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            ShowNoteFlyout(sender as FrameworkElement);
        }

        private void NotePreview_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            ShowNoteFlyout(sender as FrameworkElement);
        }

        private void ShowNoteFlyout(FrameworkElement element)
        {
            if (element == null) return;
            Flyout.ShowAttachedFlyout(element);

            openedFlyout = Flyout.GetAttachedFlyout(element);
            openedFlyout.Closed += OpenedFlyout_Closed;
        }

        private void OpenedFlyout_Closed(object sender, object e)
        {
            if(openedFlyout != null)
                openedFlyout.Closed -= OpenedFlyout_Closed;

            openedFlyout = null;
        }

        private void GridView_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            NotesGridView.ReorderMode = ListViewReorderMode.Enabled;
        }

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

        private void NotesFluidGrid_ItemsReordered(object sender, Events.ItemsReorderedEventArgs e)
        {
            var handler = ItemsReordered;
            if (handler != null) handler(sender, e);
        }

        private void NotesGridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (openedFlyout != null)
                openedFlyout.Hide();
        }

        private void NotesFluidGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
