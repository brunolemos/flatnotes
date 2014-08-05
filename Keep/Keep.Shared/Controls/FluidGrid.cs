using System;
using System.Net;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Keep.Controls
{
    public class FluidGrid : Panel
    {
        protected override Size MeasureOverride( Size totalSize )
        {
            //Debug.WriteLine( "Called MeasureOverride " + totalSize );
            int columns = Columns ;

            if ( columns < 1 )
                columns = (int)Math.Floor( totalSize.Width / ItemMinWidth );

            int i, columnWithlastY = 0;
            LastCellWidth = totalSize.Width / columns;
            double[] lastYInColumn = new double[columns];
            Size resultSize = new Size( totalSize.Width, 0 );

            for ( i = 0; i < columns; i++ )
                lastYInColumn[i] = 0;

            foreach ( UIElement child in Children )
            {
                for ( i = columns - 1; i >= 0; i-- )
                    if ( lastYInColumn[i] <= lastYInColumn[columnWithlastY] )
                        columnWithlastY = i;

                child.Measure( new Size( LastCellWidth, totalSize.Height ) );
                Size cellSize = new Size( LastCellWidth, child.DesiredSize.Height );
                Point startPoint = new Point( cellSize.Width * columnWithlastY, lastYInColumn[columnWithlastY] );
                lastYInColumn[columnWithlastY] = startPoint.Y + cellSize.Height;
            }

            if ( Double.IsPositiveInfinity( totalSize.Width ) ) totalSize.Width = 0;
            if ( Double.IsPositiveInfinity( totalSize.Height ) ) totalSize.Height = 0;

            for ( i = 0; i < columns; i++ )
                resultSize.Height = Math.Max( resultSize.Height, lastYInColumn[i] );

            //Debug.WriteLine( "Returned " + resultSize );
            return resultSize;
        }

        protected override Size ArrangeOverride( Size totalSize )
        {
            //Debug.WriteLine( "Called ArrangeOverride" + totalSize );
            int columns = Columns;

            if ( columns < 1 )
                columns = (int)Math.Floor( totalSize.Width / ItemMinWidth );

            int i, columnWithlastY = 0;
            LastCellWidth = totalSize.Width / columns;
            double[] lastYInColumn = new double[columns];

            for ( i = 0; i < columns; i++ )
                lastYInColumn[i] = 0;

            foreach ( UIElement child in Children )
            {
                for ( i = columns - 1; i >= 0; i-- )
                    if ( lastYInColumn[i] <= lastYInColumn[columnWithlastY] )
                        columnWithlastY = i;

                Size cellSize = new Size( LastCellWidth, child.DesiredSize.Height );
                Point startPoint = new Point( cellSize.Width * columnWithlastY, lastYInColumn[columnWithlastY] );
                lastYInColumn[columnWithlastY] = startPoint.Y + cellSize.Height;

                child.Arrange( new Rect( startPoint, cellSize ) );
            }

            return totalSize;
        }

        public static readonly DependencyProperty LastCellWidthProperty = DependencyProperty.Register("LastCellWidth", typeof(Double), typeof(FluidGrid), new PropertyMetadata(194.0));
        public Double LastCellWidth
        {
            get { return (Double)GetValue(LastCellWidthProperty); }
            set { SetValue(LastCellWidthProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(FluidGrid), new PropertyMetadata(1, OnColumnsChanged));
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ItemMinWidthProperty = DependencyProperty.Register("ItemMinWidth", typeof(int), typeof(FluidGrid), new PropertyMetadata(0, OnItemMinWidthChanged));
        public int ItemMinWidth
        {
            get { return (int)GetValue( ItemMinWidthProperty ); }
            set { SetValue( ItemMinWidthProperty, value ); }
        }




        static void OnColumnsChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
        {
            ( obj as FrameworkElement ).InvalidateMeasure();
            //if ( (int)e.NewValue < 1 )
            //    ( obj as FluidGrid ).Columns = 1;
        }

        static void OnItemMinWidthChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
        {
            //Debug.WriteLine( "Attention: 'ItemMinWidth' property is ignored when 'Columns' > 0." );
            ( obj as FrameworkElement ).InvalidateMeasure();

            //if ( (int)e.NewValue < 0 )
            //    ( obj as FluidGrid ).ItemMinWidth = 0;
        }
    }
}
