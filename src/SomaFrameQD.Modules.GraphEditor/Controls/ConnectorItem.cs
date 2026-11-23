using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SomaFrameQD.Framework;

namespace SomaFrameQD.Modules.GraphEditor.Controls
{
    public class ConnectorItem : ContentControl
    {
        private Point _lastMousePosition;
        private bool _isDragging;
        private bool _isLayoutUpdatedSubscribed;

        static ConnectorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ConnectorItem),
                new FrameworkPropertyMetadata(typeof(ConnectorItem)));
        }

        #region Dependency properties

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            "Position", typeof(Point), typeof(ConnectorItem));

        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        #endregion

        #region Routed events

        internal static readonly RoutedEvent ConnectorDragStartedEvent = EventManager.RegisterRoutedEvent(
            "ConnectorDragStarted", RoutingStrategy.Bubble, typeof(ConnectorItemDragStartedEventHandler),
            typeof(ConnectorItem));

        internal static readonly RoutedEvent ConnectorDraggingEvent = EventManager.RegisterRoutedEvent(
            "ConnectorDragging", RoutingStrategy.Bubble, typeof(ConnectorItemDraggingEventHandler),
            typeof(ConnectorItem));

        internal static readonly RoutedEvent ConnectorDragCompletedEvent = EventManager.RegisterRoutedEvent(
            "ConnectorDragCompleted", RoutingStrategy.Bubble, typeof(ConnectorItemDragCompletedEventHandler),
            typeof(ConnectorItem));

        #endregion

        private GraphControl ParentGraphControl
        {
            get { return VisualTreeUtility.FindParent<GraphControl>(this); }
        }

        internal ElementItem ParentElementItem
        {
            get { return VisualTreeUtility.FindParent<ElementItem>(this); }
        }

        public ConnectorItem()
        {
            // Gate global Dispatcher ContextLayoutManager subscriptions on Loaded/Unloaded so closed
            // graph documents do not remain rooted by LayoutUpdated listeners in .NET Framework 4.8.
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isLayoutUpdatedSubscribed)
            {
                LayoutUpdated += OnLayoutUpdated;
                _isLayoutUpdatedSubscribed = true;
            }

            UpdatePosition();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_isLayoutUpdatedSubscribed)
            {
                LayoutUpdated -= OnLayoutUpdated;
                _isLayoutUpdatedSubscribed = false;
            }
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var arranged = base.ArrangeOverride(arrangeBounds);
            UpdatePosition();
            return arranged;
        }

        /// <summary>
        /// Computes the coordinates, relative to the parent <see cref="GraphControl" />, of this connector.
        /// This is used to correctly position any connections that may be connected to this connector.
        /// </summary>
        private void UpdatePosition()
        {
            if (PresentationSource.FromVisual(this) == null)
                return;

            var parentGraphControl = VisualTreeUtility.FindParent<GraphControl>(this);
            if (parentGraphControl == null)
                return;

            double width = ActualWidth > 0.0 ? ActualWidth : RenderSize.Width;
            double height = ActualHeight > 0.0 ? ActualHeight : RenderSize.Height;
            var centerPoint = new Point(width / 2.0, height / 2.0);
            var newPosition = TransformToAncestor(parentGraphControl).Transform(centerPoint);
            if (Position != newPosition)
            {
                Position = newPosition;
            }
        }

        #region Mouse input

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            ParentElementItem?.Focus();

            var parentGraph = ParentGraphControl;
            if (parentGraph != null)
            {
                _lastMousePosition = e.GetPosition(parentGraph);
            }

            e.Handled = true;

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_isDragging)
                {
                    var parentGraph = ParentGraphControl;
                    if (parentGraph != null)
                    {
                        var currentMousePosition = e.GetPosition(parentGraph);
                        var offset = currentMousePosition - _lastMousePosition;

                        _lastMousePosition = currentMousePosition;

                        RaiseEvent(new ConnectorItemDraggingEventArgs(
                            ConnectorDraggingEvent,
                            this,
                            offset.X,
                            offset.Y));
                    }
                }
                else
                {
                    var eventArgs = new ConnectorItemDragStartedEventArgs(ConnectorDragStartedEvent, this);
                    RaiseEvent(eventArgs);

                    if (eventArgs.Cancel)
                        return;

                    _isDragging = true;
                    CaptureMouse();
                }

                e.Handled = true;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                RaiseEvent(new ConnectorItemDragCompletedEventArgs(ConnectorDragCompletedEvent, this));
                ReleaseMouseCapture();
                _isDragging = false;
                e.Handled = true;
            }

            base.OnMouseLeftButtonUp(e);
        }

        #endregion
    }
}
