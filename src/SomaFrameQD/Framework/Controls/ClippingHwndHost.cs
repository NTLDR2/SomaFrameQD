using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using SomaFrameQD.Framework.Win32;

namespace SomaFrameQD.Framework.Controls
{
    // Why ClippingHwndHost exists in SomaFrameQD:
    // In a multi-pane IDE shell (AvalonDock splitters, tool panes, and embedded HwndHost viewports),
    // Win32 child HWNDs enforce hard OS-level rectangular clipping and z-order ("airspace") rules that
    // ordinary WPF Visual clip geometries cannot override when sibling native HWNDs overlap.
    // ClippingHwndHost hosts a WPF Visual subtree (Content) inside its own dedicated WS_CHILD | WS_VISIBLE
    // Win32 HwndSource ("SomaFrameQDClippingHwndHost") so the OS window manager clips the visual strictly
    // to the pane's Win32 HWND rectangle across Windows 7 SP1 through Windows 11.
    public class ClippingHwndHost : HwndHost
    {
        private HwndSource _source;

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(Visual), typeof(ClippingHwndHost),
            new PropertyMetadata(OnContentChanged));

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var hwndHost = (ClippingHwndHost)d;

            if (e.OldValue != null)
            {
                if (hwndHost._source != null)
                    hwndHost._source.RootVisual = null;
                hwndHost.RemoveLogicalChild(e.OldValue);
            }

            if (e.NewValue != null)
            {
                hwndHost.AddLogicalChild(e.NewValue);
                if (hwndHost._source != null)
                    hwndHost._source.RootVisual = (Visual)e.NewValue;
            }
        }

        public Visual Content
        {
            get { return (Visual)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (Content != null)
                    yield return Content;
            }
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            // FrameworkElement.Width and Height default to double.NaN in WPF. Converting double.NaN directly
            // to int produces 0x80000000 (CW_USEDEFAULT), which is invalid for WS_CHILD HwndSource windows.
            // Use safe >= 0 pixel bounds; OnRenderSizeChanged -> UpdateWindowPos() syncs subsequent layout sizes.
            int initialWidth = GetSafeChildDimension(ActualWidth, Width);
            int initialHeight = GetSafeChildDimension(ActualHeight, Height);

            var param = new HwndSourceParameters("SomaFrameQDClippingHwndHost", initialWidth, initialHeight)
            {
                ParentWindow = hwndParent.Handle,
                WindowStyle = NativeMethods.WS_VISIBLE | NativeMethods.WS_CHILD,
            };

            _source = new HwndSource(param)
            {
                RootVisual = Content
            };

            return new HandleRef(this, _source.Handle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            if (_source != null)
            {
                _source.RootVisual = null;
                _source.Dispose();
                _source = null;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            // Ensure HwndHost immediately synchronizes the child HwndSource rectangle when WPF layout resizes.
            UpdateWindowPos();

            base.OnRenderSizeChanged(sizeInfo);
        }

        private static int GetSafeChildDimension(double actualLength, double requestedLength)
        {
            if (!double.IsNaN(actualLength) && !double.IsInfinity(actualLength) && actualLength > 0.0)
            {
                return (int)Math.Round(actualLength);
            }

            if (!double.IsNaN(requestedLength) && !double.IsInfinity(requestedLength) && requestedLength > 0.0)
            {
                return (int)Math.Round(requestedLength);
            }

            return 0;
        }
    }
}
