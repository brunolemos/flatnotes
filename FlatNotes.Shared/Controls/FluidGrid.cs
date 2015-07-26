using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace FlatNotes.Controls
{
    public class FluidGrid : Panel
    {
        private int[] childrenColumns;
        private Size[] childrenSizes;
        private double itemWidth = 160;
        private int columns = -1;

        List<UIElement> elements;
        private bool isReordering = false;
        private int draggingItemIndex = -1;
        private int dropAtIndex = -1;

        public FluidGrid()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("Loaded " + ParentListView);
            if (ParentListView != null)
            {
                ParentListView.DragItemsStarting += OnDragItemsStarting;
                ParentListView.DragOver += OnDragOver;
                ParentListView.Drop += OnDrop;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("Unloaded " + ParentListView);
            if (ParentListView != null)
            {
                ParentListView.DragItemsStarting -= OnDragItemsStarting;
                ParentListView.DragOver -= OnDragOver;
                ParentListView.Drop -= OnDrop;
            }
        }

        protected override Size MeasureOverride(Size totalSize)
        {
            //Debug.WriteLine("MeasureOverride");
            itemWidth = Math.Min(ItemWidth, totalSize.Width);
            columns = Columns < 1 || (Columns == 1 && !AllowSingleColumn)
                    ? Math.Max(1, (int)Math.Floor(totalSize.Width / itemWidth))
                    : Columns;

            //adjust item width when itemwidth is too big
            itemWidth = Math.Min(itemWidth, totalSize.Width / columns);

            //stretch on force or when when single column on small screen
            if (ItemStretch || columns == 1) itemWidth = totalSize.Width / columns; // && totalSize.Width < itemWidth * 2

            //System.Diagnostics.Debug.WriteLine("MeasureOverride ItemWidth: {0}, Stretch: {1}, Columns: {2}", itemWidth, ItemStretch, columns);
            Size resultSize = new Size(columns * itemWidth, 100);

            if (elements == null) elements = new List<UIElement>();
            if (!isReordering) elements.Clear();

            if (elements.Count <= 0)
                foreach (var item in Children)
                    elements.Add(item);

            if (isReordering && draggingItemIndex >= 0 && dropAtIndex >= 0 && draggingItemIndex < elements.Count && dropAtIndex < elements.Count)
            {
                var _draggingItemIndex = elements.IndexOf(Children[draggingItemIndex]);
                var _dropAtIndex = elements.IndexOf(Children[dropAtIndex]);

                var temp = elements[_draggingItemIndex];
                elements.Remove(temp);
                elements.Insert(_dropAtIndex, temp);
            }

            int columnWithLowerY = 0;
            BiggerItemHeight = 0;

            double[] lastYInColumn = new double[columns];
            for (int i = 0; i < columns; i++) lastYInColumn[i] = 0;

            childrenColumns = new int[elements.Count];
            childrenSizes = new Size[elements.Count];

            for (int pos = 0; pos < elements.Count; pos++)
            {
                for (int i = columns - 1; i >= 0; i--)
                    if (lastYInColumn[i] <= lastYInColumn[columnWithLowerY])
                        columnWithLowerY = i;

                elements[pos].Measure(new Size(itemWidth, totalSize.Height));
                Size itemSize = new Size(itemWidth, elements[pos].DesiredSize.Height);
                if (Double.IsPositiveInfinity(itemSize.Width)) itemSize.Width = 0;
                if (Double.IsPositiveInfinity(itemSize.Height)) itemSize.Height = 0;

                lastYInColumn[columnWithLowerY] = lastYInColumn[columnWithLowerY] + itemSize.Height;

                childrenColumns[pos] = columnWithLowerY;
                childrenSizes[pos] = itemSize;
                BiggerItemHeight = Math.Max(BiggerItemHeight, itemSize.Height);
            }

            if (Double.IsPositiveInfinity(resultSize.Width)) resultSize.Width = 0;
            if (Double.IsPositiveInfinity(resultSize.Height)) resultSize.Height = 0;

            for (int i = 0; i < columns; i++)
                resultSize.Height = Math.Max(resultSize.Height, lastYInColumn[i]);

            return resultSize;
        }

        protected override Size ArrangeOverride(Size totalSize)
        {
            //Debug.WriteLine("ArrangeOverride");

            if (elements == null) elements = new List<UIElement>();
            if (!isReordering) elements.Clear();

            if (elements.Count <= 0)
                foreach (var item in Children)
                    elements.Add(item);

            if (childrenColumns.Length != elements.Count || childrenSizes.Length != elements.Count) return totalSize;

            double[] lastYInColumn = new double[columns];

            for (int i = 0; i < columns; i++)
                lastYInColumn[i] = 0;

            //#if WINDOWS_UAP
            //            UIElement[] lastElementInColumn = new UIElement[columns];

            //            for (int i = 0; i < elements.Count; i++)
            //            {
            //                var item = elements[i];

            //                if (i < columns)
            //                {
            //                    lastElementInColumn[i] = item;
            //                    if (i > 0) RelativePanel.SetRightOf(item, elements[i - 1]);
            //                    continue;
            //                }

            //                Size childSize = childrenSizes[i];
            //                int childColumn = childrenColumns[i];

            //                RelativePanel.SetBelow(item, lastElementInColumn[childColumn]);
            //                RelativePanel.SetAlignHorizontalCenterWith(item, lastElementInColumn[childColumn]);

            //                lastElementInColumn[childColumn] = item;
            //                lastYInColumn[childColumn] += childSize.Height;
            //            }
            //#else
            for (int i = 0; i < elements.Count; i++)
            {
                Size childSize = childrenSizes[i];
                int childColumn = childrenColumns[i];

                Point startPoint = new Point(childSize.Width * childColumn, lastYInColumn[childColumn]);
                elements[i].Arrange(new Rect(startPoint, childSize));

                lastYInColumn[childColumn] += childSize.Height;
            }
            //#endif

            return totalSize;
        }

        public static readonly DependencyProperty ParentListViewProperty = DependencyProperty.Register("ParentListView", typeof(ListViewBase), typeof(FluidGrid), new PropertyMetadata(null));
        public ListViewBase ParentListView
        {
            get { return (ListViewBase)GetValue(ParentListViewProperty); }
            set { SetValue(ParentListViewProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(FluidGrid), new PropertyMetadata(-1, OnPropertyChanged));
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(FluidGrid), new PropertyMetadata(160.0, OnPropertyChanged));
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        public static readonly DependencyProperty ItemStretchProperty = DependencyProperty.Register("ItemStretch", typeof(bool), typeof(FluidGrid), new PropertyMetadata(false, OnPropertyChanged));
        public bool ItemStretch
        {
            get { return (bool)GetValue(ItemStretchProperty); }
            set { SetValue(ItemStretchProperty, value); }
        }

        public static readonly DependencyProperty AllowSingleColumnProperty = DependencyProperty.Register("AllowSingleColumn", typeof(bool), typeof(FluidGrid), new PropertyMetadata(true, OnPropertyChanged));
        public bool AllowSingleColumn
        {
            get { return (bool)GetValue(AllowSingleColumnProperty); }
            set { SetValue(AllowSingleColumnProperty, value); }
        }

        public static readonly DependencyProperty BiggerItemHeightProperty = DependencyProperty.Register("BiggerItemHeight", typeof(double), typeof(FluidGrid), new PropertyMetadata(0));
        public double BiggerItemHeight
        {
            get { return (double)GetValue(BiggerItemHeightProperty); }
            private set { SetValue(BiggerItemHeightProperty, value); }
        }

        static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("OnPropertyChanged ItemWidth: {0}, Stretch: {1}, Columns: {2}", (obj as FluidGrid).itemWidth, (obj as FluidGrid).ItemStretch, (obj as FluidGrid).Columns);
            (obj as FluidGrid).InvalidateMeasure();
        }

        private void OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            isReordering = false;

#if WINDOWS_UAP
            e.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
#endif

            draggingItemIndex = -1;
            if (e.Items.Count < 1) return;

            var itemContainer = (sender as ListViewBase).ContainerFromItem(e.Items[0]) as UIElement;
            if (itemContainer == null) return;

            draggingItemIndex = (sender as ListViewBase).IndexFromContainer(itemContainer);
            //draggingItemIndex = elements.IndexOf(itemContainer);
            Debug.WriteLine("OnDragItemsStarting. Sender: {0}, Item: {1} = {2}", sender, draggingItemIndex, e.Items[0]);

            isReordering = true;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (!isReordering || draggingItemIndex == -1) return;

            int oldDropIndex = dropAtIndex;

            //Get the list of items under the current position
            var position = e.GetPosition(null);
            var hitContainers = VisualTreeHelper.FindElementsInHostCoordinates(position, this).OfType<SelectorItem>().ToArray();

            dropAtIndex = (hitContainers == null || hitContainers.Count() <= 0)
                            ? -1
                            : ParentListView.IndexFromContainer(hitContainers[0]);//elements.IndexOf(hitContainers[0]);

            if (dropAtIndex == oldDropIndex || dropAtIndex == draggingItemIndex) return;

            if (dropAtIndex < 0) return;
            Debug.WriteLine("OnDragOver. Drag: {0}, Drop: {1}, OldDrop: {2}", draggingItemIndex, dropAtIndex, oldDropIndex);
            e.Handled = true;

#if WINDOWS_UAP
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
#endif
            InvalidateMeasure();
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            Debug.WriteLine("OnDrop. Drag: {0}, Drop: {1}", draggingItemIndex, dropAtIndex);
            isReordering = false;

            if (draggingItemIndex == -1 || dropAtIndex == -1) return;
            if (draggingItemIndex >= Children.Count || dropAtIndex >= Children.Count) return;

            draggingItemIndex = -1;
            dropAtIndex = -1;
        }
    }
}
