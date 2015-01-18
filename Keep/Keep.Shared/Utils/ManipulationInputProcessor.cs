using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Input;
using System.Diagnostics;
using System;

namespace Keep.Utils
{
    class ManipulationInputProcessor
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
        int pointerMovedCount = 0;
        bool wasPointerPressedCalled;

        public ManipulationInputProcessor(GestureRecognizer gr, FrameworkElement target, FrameworkElement referenceframe)
        {
            this.gestureRecognizer = gr;
            this.element = target;
            this.reference = referenceframe;

            this.elementInitialOpacity = element.Opacity;
            this.containerSize = new Size(referenceframe.ActualWidth, 0);

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
            element.Opacity = elementInitialOpacity;
        }

        public void Disable()
        {
            if (element != null) element.ManipulationMode = previousManipulationModes;
            if (element != null) element.Opacity = elementInitialOpacity;
            UnhandleEvents();
        }

        private void UnhandleEvents()
        {
            // Set down pointer event handlers
            this.element.PointerCanceled -= OnPointerCanceled;
            //this.element.PointerPressed -= OnPointerPressed;
            this.element.PointerReleased -= OnPointerReleased;
            this.element.PointerMoved -= OnPointerMoved;

            // Set down event handlers to respond to gesture recognizer output
            this.gestureRecognizer.ManipulationStarted -= OnManipulationStarted;
            this.gestureRecognizer.ManipulationUpdated -= OnManipulationUpdated;
            this.gestureRecognizer.ManipulationCompleted -= OnManipulationCompleted;
        }

        void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            // Route the events to the gesture recognizer
            // The points are in the reference frame of the canvas that contains the rectangle element.
            this.gestureRecognizer.ProcessDownEvent(args.GetCurrentPoint(this.reference));

            // Set the pointer capture to the element being interacted with
            this.element.CapturePointer(args.Pointer);
            wasPointerPressedCalled = true;

            // Mark the event handled to prevent execution of default handlers
            args.Handled = true;
        }

        void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            this.gestureRecognizer.ProcessUpEvent(args.GetCurrentPoint(this.reference));
            args.Handled = true;

            pointerMovedCount = 0;
            wasPointerPressedCalled = false;
        }

        void OnPointerCanceled(object sender, PointerRoutedEventArgs args)
        {
            this.gestureRecognizer.CompleteGesture();
            args.Handled = true;
        }

        void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            args.Handled = true;

            //workarround to enable tap events / focus on textbox / ...
            if (pointerMovedCount <= 2) pointerMovedCount++;
            if (pointerMovedCount == 2 && !wasPointerPressedCalled) OnPointerPressed(sender, args);
            if(pointerMovedCount < 3) return;

            this.gestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(this.reference));
        }

        public void InitializeTransforms()
        {
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

            element.Opacity = Math.Round(100 - 100 * Math.Abs(e.Cumulative.Translation.X) / (containerSize.Width * 0.8)) / 100;
            if (element.Opacity <= 0) this.gestureRecognizer.CompleteGesture();
        }

        void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (element.Opacity <= 0)
            {
                Disable();

                var handler = ItemSwiped;
                if (handler != null) handler(element, EventArgs.Empty);

                return;
            }

            ResetPosition();
        }
    }
}
