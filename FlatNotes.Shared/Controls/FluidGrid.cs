using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.Controls
{
    public class FluidGrid : Panel
    {
        private int[] childrenColumns;
        private Size[] childrenSizes;
        private double itemWidth = 150;

        protected override Size MeasureOverride(Size totalSize)
        {
            itemWidth = Math.Min(ItemWidth, totalSize.Width);
            int columns = Columns >= 1 ? Columns : Math.Max(1, (int)Math.Floor(totalSize.Width / itemWidth));

            //adjust item width when itemwidth is too big
            itemWidth = Math.Min(itemWidth, totalSize.Width / columns);
            System.Diagnostics.Debug.WriteLine("MeasureOverride ItemWidth: {0}, Stretch: {1}, Columns: {2}", itemWidth, ItemStretch, columns);

            //stretch on force or when when single column on small screen
            if (ItemStretch || columns == 1) itemWidth = totalSize.Width / columns; // && totalSize.Width < itemWidth * 2

            Size resultSize = new Size(columns * itemWidth, 100);

            int i, columnWithLowerY = 0;
            BiggerItemHeight= 0;

            double[] lastYInColumn = new double[columns];
            for (i = 0; i < columns; i++) lastYInColumn[i] = 0;

            childrenColumns = new int[Children.Count];
            childrenSizes = new Size[Children.Count];

            for (int pos = 0; pos < Children.Count; pos++)
            {
                for (i = columns - 1; i >= 0; i--)
                    if (lastYInColumn[i] <= lastYInColumn[columnWithLowerY])
                        columnWithLowerY = i;

                Children[pos].Measure(new Size(itemWidth, totalSize.Height));
                Size itemSize = new Size(itemWidth, Children[pos].DesiredSize.Height);
                if (Double.IsPositiveInfinity(itemSize.Width)) itemSize.Width = 0;
                if (Double.IsPositiveInfinity(itemSize.Height)) itemSize.Height = 0;

                lastYInColumn[columnWithLowerY] = lastYInColumn[columnWithLowerY] + itemSize.Height;

                childrenColumns[pos] = columnWithLowerY;
                childrenSizes[pos] = itemSize;
                BiggerItemHeight = Math.Max(BiggerItemHeight, itemSize.Height);
            }

            if (Double.IsPositiveInfinity(resultSize.Width)) resultSize.Width = 0;
            if (Double.IsPositiveInfinity(resultSize.Height)) resultSize.Height = 0;

            for (i = 0; i < columns; i++)
                resultSize.Height = Math.Max(resultSize.Height, lastYInColumn[i]);

            return resultSize;
        }

        protected override Size ArrangeOverride( Size totalSize )
        {
            if (childrenColumns.Length != Children.Count || childrenSizes.Length != Children.Count) return totalSize;
            
            int columns = Columns >= 1 ? Columns : Math.Max(1, (int)Math.Floor(totalSize.Width / itemWidth));

            double[] lastYInColumn = new double[columns];
            for (int i = 0; i < columns; i++) lastYInColumn[i] = 0;

            for (int pos = 0; pos < Children.Count; pos++)
            {
                Size childSize = childrenSizes[pos];
                int childColumn = childrenColumns[pos];

                Point startPoint = new Point(childSize.Width * childColumn, lastYInColumn[childColumn]);
                Children[pos].Arrange(new Rect(startPoint, childSize));

                lastYInColumn[childColumn] += childSize.Height;
            }

            return totalSize;
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(FluidGrid), new PropertyMetadata(-1, OnPropertyChanged));
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(FluidGrid), new PropertyMetadata(150, OnPropertyChanged));
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

        public static readonly DependencyProperty BiggerItemHeightProperty = DependencyProperty.Register("BiggerItemHeight", typeof(double), typeof(FluidGrid), new PropertyMetadata(0));
        public double BiggerItemHeight
        {
            get { return (double)GetValue(BiggerItemHeightProperty); }
            private set { SetValue(BiggerItemHeightProperty, value); }
        }

        static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnPropertyChanged ItemWidth: {0}, Stretch: {1}, Columns: {2}", (obj as FluidGrid).itemWidth, (obj as FluidGrid).ItemStretch, (obj as FluidGrid).Columns);
            (obj as FluidGrid).InvalidateMeasure();
        }
    }
}
