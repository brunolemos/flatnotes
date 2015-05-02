using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.Controls
{
    public class FluidGrid : Panel
    {
        int[] childrenColumns;
        Size[] childrenSizes;

        protected override Size MeasureOverride(Size totalSize)
        {
            int columns = Columns;
            if (columns < 1) columns = Math.Max( (int)Math.Floor(totalSize.Width / ItemMinWidth), 1);

            int i, columnWithlastY = 0;
            LastCellWidth = totalSize.Width / columns;
            Size resultSize = new Size(totalSize.Width, 0);
            BiggerCellHeight= 0;

            double[] lastYInColumn = new double[columns];
            for (i = 0; i < columns; i++) lastYInColumn[i] = 0;

            childrenColumns = new int[Children.Count];
            childrenSizes = new Size[Children.Count];
            for (int pos = 0; pos < Children.Count; pos++)
            {
                for (i = columns - 1; i >= 0; i--)
                    if (lastYInColumn[i] <= lastYInColumn[columnWithlastY])
                        columnWithlastY = i;

                Children[pos].Measure(new Size(LastCellWidth, totalSize.Height));
                Size cellSize = new Size(LastCellWidth, Children[pos].DesiredSize.Height);
                if (Double.IsPositiveInfinity(cellSize.Width)) cellSize.Width = 0;
                if (Double.IsPositiveInfinity(cellSize.Height)) cellSize.Height = 0;

                Point startPoint = new Point(cellSize.Width * columnWithlastY, lastYInColumn[columnWithlastY]);
                lastYInColumn[columnWithlastY] = startPoint.Y + cellSize.Height;

                childrenColumns[pos] = columnWithlastY;
                childrenSizes[pos] = cellSize;
                BiggerCellHeight = Math.Max(BiggerCellHeight, cellSize.Height);
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

            int columns = Columns;
            if (columns < 1) columns = Math.Max( (int)Math.Floor(totalSize.Width / ItemMinWidth), 1);

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

        public static readonly DependencyProperty BiggerCellHeightProperty = DependencyProperty.Register("BiggerCellHeight", typeof(Double), typeof(FluidGrid), new PropertyMetadata(0));
        public Double BiggerCellHeight
        {
            get { return (Double)GetValue(BiggerCellHeightProperty); }
            set { SetValue(BiggerCellHeightProperty, value); }
        }

        public static readonly DependencyProperty LastCellWidthProperty = DependencyProperty.Register("LastCellWidth", typeof(Double), typeof(FluidGrid), new PropertyMetadata(194.0));
        public Double LastCellWidth
        {
            get { return (Double)GetValue(LastCellWidthProperty); }
            set { SetValue(LastCellWidthProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(FluidGrid), new PropertyMetadata(-1, OnColumnsChanged));
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ItemMinWidthProperty = DependencyProperty.Register("ItemMinWidth", typeof(int), typeof(FluidGrid), new PropertyMetadata(200, OnItemMinWidthChanged));
        public int ItemMinWidth
        {
            get { return (int)GetValue( ItemMinWidthProperty ); }
            set { SetValue( ItemMinWidthProperty, value ); }
        }

        static void OnColumnsChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
        {
            ( obj as FrameworkElement ).InvalidateMeasure();
        }

        static void OnItemMinWidthChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
        {
            ( obj as FrameworkElement ).InvalidateMeasure();
        }
    }
}
