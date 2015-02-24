using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Input;
using System.Diagnostics;
using System;

namespace Keep.Utils
{
    public class ManipulationInputProcessor
    {
        public event EventHandler ItemSwiped;

        GestureRecognizer gestureRecognizer;
        TransformGroup cumulativeTransform;
        MatrixTransform previousTransform;
        CompositeTransform deltaTransform;
        public FrameworkElement element;
        FrameworkElement reference;
        ManipulationModes previousManipulationModes;
        double elementInitialOpacity = 1;
        Size containerSize;
        bool wasPointerPressedCalled;
        int pointerMovedCount = 0;
        double minOpacity = 0.20;

        public ManipulationInputProcessor(GestureRecognizer gr, FrameworkElement target, FrameworkElement referenceframe)
        {
            if (gr == null || target == null) throw new InvalidOperationException("You must provide valid arguments");

            this.gestureRecognizer = gr;
            this.element = target;
            this.reference = referenceframe != null ? referenceframe : target;

            //this.elementInitialOpacity = 1;// element.Opacity;
            this.containerSize = new Size(target.ActualWidth, 0);

            previousManipulationModes = element.ManipulationMode;
            element.ManipulationMode = ManipulationModes.System |
                                       ManipulationModes.TranslateX |
                                       ManipulationModes.TranslateInertia;

            this.gestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateX |
                                                     GestureSettings.ManipulationTranslateInertia;

            // Set up pointer event handlers. These receive input events that are used by the gesture recognizer.
            this.element.PointerCanceled += OnPointerCanceled;
            //this.element.PointerPressed += OnPointerPressed;
            this.element.PointerReleased += OnPointerReleased;
            this.element.PointerCaptureLost += OnPointerCaptureLost;
            this.element.PointerMoved += OnPointerMoved;

            // Set up event handlers to respond to gesture recognizer output
            this.gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            this.gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            this.gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;

            InitializeTransforms();
        }

        public void ResetPosition()
        {
            InitializeTransforms();

            if(element != null) element.Opacity = elementInitialOpacity;

            pointerMovedCount = 0;
            wasPointerPressedCalled = false;
        }

        public void Disable()
        {
            if (element == null) return;

            ResetPosition();

            element.ManipulationMode = previousManipulationModes;
            UnhandleEvents();
        }

        private void UnhandleEvents()
        {
            if (element != null)
            {
                // Set down pointer event handlers
                this.element.PointerCanceled -= OnPointerCanceled;
                //this.element.PointerPressed -= OnPointerPressed;
                this.element.PointerReleased -= OnPointerReleased;
                this.element.PointerCaptureLost -= OnPointerCaptureLost;
                this.element.PointerMoved -= OnPointerMoved;
            }

            if(gestureRecognizer != null)
            {
                // Set down event handlers to respond to gesture recognizer output
                this.gestureRecognizer.ManipulationStarted -= OnManipulationStarted;
                this.gestureRecognizer.ManipulationUpdated -= OnManipulationUpdated;
                this.gestureRecognizer.ManipulationCompleted -= OnManipulationCompleted;
            }
        }

        void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Route the events to the gesture recognizer
            // The points are in the reference frame of the canvas that contains the rectangle element.
            this.gestureRecognizer.ProcessDownEvent(e.GetCurrentPoint(this.reference));

            // Set the pointer capture to the element being interacted with
            this.element.CapturePointer(e.Pointer);
            wasPointerPressedCalled = true;

            // Mark the event handled to prevent execution of default handlers
            e.Handled = true;
        }

        void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            this.gestureRecognizer.ProcessUpEvent(e.GetCurrentPoint(this.reference));
            e.Handled = true;

            pointerMovedCount = 0;
            wasPointerPressedCalled = false;
        }

        void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            this.gestureRecognizer.CompleteGesture();
            e.Handled = true;

            pointerMovedCount = 0;
            wasPointerPressedCalled = false;

            ResetPosition();
        }

        void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            if (!e.Pointer.IsInContact) return;

            this.gestureRecognizer.CompleteGesture();

            pointerMovedCount = 0;
            wasPointerPressedCalled = false;

            ResetPosition();
        }

        void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            //workarround to enable tap events / focus on textbox / ...
            if (pointerMovedCount <= 2) pointerMovedCount++;
            if (pointerMovedCount == 2 && !wasPointerPressedCalled) { OnPointerPressed(sender, e); return; }

            this.gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(this.reference));
        }

        public void InitializeTransforms()
        {
            if (element == null) return;

            this.cumulativeTransform = new TransformGroup();
            this.deltaTransform = new CompositeTransform();
            this.previousTransform = new MatrixTransform() { Matrix = Matrix.Identity };

            this.cumulativeTransform.Children.Add(previousTransform);
            this.cumulativeTransform.Children.Add(deltaTransform);

            this.element.RenderTransform = this.cumulativeTransform;
        }

        void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
        }

        void OnManipulationUpdated(object sender, ManipulationUpdatedEventArgs e)
        {
            this.previousTransform.Matrix = this.cumulativeTransform.Value;

            Point center = new Point(e.Position.X, e.Position.Y);

            this.deltaTransform.CenterX = center.X;
            this.deltaTransform.CenterY = center.Y;

            this.deltaTransform.TranslateX = e.Delta.Translation.X;

            double newOpacity = Math.Round(100 - 100 * Math.Abs(e.Cumulative.Translation.X) / (containerSize.Width * 0.6)) / 100;
            if (double.IsNaN(newOpacity) || (newOpacity <= minOpacity && !gestureRecognizer.IsInertial)) newOpacity = minOpacity;
            element.Opacity = newOpacity;

            if (newOpacity <= 0 && gestureRecognizer.IsInertial) this.gestureRecognizer.CompleteGesture();
        }

        void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (element.Opacity <= minOpacity)
            {
                Disable();
                element.Opacity = 0;

                var handler = ItemSwiped;
                if (handler != null) handler(element, EventArgs.Empty);

                return;
            }

            ResetPosition();
        }
    }
}
