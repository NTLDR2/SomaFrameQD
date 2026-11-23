#region File Description
//-----------------------------------------------------------------------------
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL: 
// http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using SomaFrameQD.Framework.Win32;

namespace SomaFrameQD.Framework.Controls
{
    // Why HwndWrapper exists in SomaFrameQD:
    // Standard WPF renders an entire top-level Window inside a single DirectX-composited Win32 HWND,
    // which means external native rendering engines (Direct3D 9/11/12 swapchains, OpenGL/Vulkan contexts,
    // scientific/CAD viewports, or native C++ engines) have no sub-window HWND handle to present frames into.
    // HwndWrapper subclasses WPF's HwndHost to allocate a dedicated WS_CHILD | WS_VISIBLE Win32 child HWND
    // inside any dockable AvalonDock document or tool pane.
    //
    // Why HwndWrapper also wraps Win32 mouse, focus, and capture messages:
    // Because Win32 enforces strict rectangular "HWND airspace" isolation, WPF's normal routed mouse events
    // (MouseMove, MouseWheel, MouseDown/Up) do not originate from or bubble out of non-WPF child HWNDs.
    // HwndWrapper intercepts raw Win32 WndProc messages on the hosted child HWND, tracks Win32 mouse capture
    // and focus transitions across application activation changes, and surfaces strongly typed .NET
    // HwndMouseEventArgs events using 64-bit (x64) safe WPARAM/LPARAM unpacking across Windows 7 SP1 - Windows 11.

    /// <summary>
    /// A control that enables graphics rendering inside a WPF control through
    /// the use of a hosted child Hwnd.
    /// </summary>
    public abstract class HwndWrapper : HwndHost
    {
        #region Fields

        // The name of our Win32 child window class registered once per process.
        private const string WindowClass = "GraphicsDeviceControlHostWindowClass";

        private static readonly object WindowClassSyncRoot = new object();
        private static bool _windowClassRegistered;

        // The child HWND handle presented to by derived native/GPU renderers.
        private IntPtr _hWnd;

        // Previously focused HWND saved on mouse-enter and restored on mouse-leave.
        private IntPtr _hWndPrev;

        // Tracks whether the host WPF application currently holds foreground activation.
        private bool _applicationHasFocus;

        // Tracks whether the mouse pointer is currently inside the child HWND bounds.
        private bool _mouseInWindow;

        // Tracks the last reported screen-space mouse position to suppress duplicate move events.
        private Point _previousPosition;

        // Mutable snapshot of button states and screen position passed into HwndMouseEventArgs.
        private readonly HwndMouseState _mouseState = new HwndMouseState();

        // Tracks whether Win32 SetCapture is active on the hosted child HWND.
        private bool _isMouseCaptured;

        // Tracks active CompositionTarget.Rendering subscription so VSync callbacks fire only
        // while the control is loaded, visible, and backed by a live child HWND.
        private bool _isRenderingSubscribed;

        // Tracks whether Dispose() has been called so late Loaded/IsVisibleChanged events never re-subscribe.
        private bool _isDisposed;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the control receives a left mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDown;

        /// <summary>
        /// Invoked when the control receives a left mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonUp;

        /// <summary>
        /// Invoked when the control receives a left mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a right mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDown;

        /// <summary>
        /// Invoked when the control receives a right mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonUp;

        /// <summary>
        /// Invoked when the control receives a rigt mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a middle mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDown;

        /// <summary>
        /// Invoked when the control receives a middle mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonUp;

        /// <summary>
        /// Invoked when the control receives a middle mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse down message for the first extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDown;

        /// <summary>
        /// Invoked when the control receives a mouse up message for the first extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonUp;

        /// <summary>
        /// Invoked when the control receives a double click message for the first extra mouse button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse down message for the second extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDown;

        /// <summary>
        /// Invoked when the control receives a mouse up message for the second extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonUp;

        /// <summary>
        /// Invoked when the control receives a double click message for the first extra mouse button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse move message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseMove;

        /// <summary>
        /// Invoked when the control first gets a mouse move message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseEnter;

        /// <summary>
        /// Invoked when the control gets a mouse leave message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseLeave;

        /// <summary>
        /// Invoked when the control recieves a mouse wheel delta.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseWheel;

        #endregion

        #region Properties

        public new bool IsMouseCaptured
        {
            get { return _isMouseCaptured; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the host application currently has foreground focus
        /// for processing Win32 mouse messages on the hosted child HWND.
        /// </summary>
        protected bool ApplicationHasFocus
        {
            get { return _applicationHasFocus; }
            set { _applicationHasFocus = value; }
        }

        internal bool IsRenderingSubscribed
        {
            get { return _isRenderingSubscribed; }
        }

        #endregion

        #region Construction and Disposal

        protected HwndWrapper()
        {
            if (Application.Current != null)
            {
                // Subscribe to application foreground changes so mouse capture and pressed button states
                // are cleanly released if the user Alt-Tabs away while dragging inside the child HWND.
                Application.Current.Activated += OnApplicationActivated;
                Application.Current.Deactivated += OnApplicationDeactivated;

                if (Application.Current.Windows.Cast<Window>().Any(x => x.IsActive))
                    _applicationHasFocus = true;
            }
            else
            {
                // When hosted in an isolated Window without an Application singleton (e.g., automated STA tests),
                // allow mouse message translation immediately.
                _applicationHasFocus = true;
            }

            // Gate CompositionTarget.Rendering on Loaded + IsVisible + live HWND so hidden tabs and collapsed
            // tool panes do not wake WPF's MediaContext at 60-240 Hz while offscreen or inactive.
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            IsVisibleChanged += OnIsVisibleChanged;
            UpdateRenderingSubscription();
        }

        protected override void Dispose(bool disposing)
        {
            _isDisposed = true;
            UpdateRenderingSubscription();

            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
            IsVisibleChanged -= OnIsVisibleChanged;

            if (Application.Current != null)
            {
                Application.Current.Activated -= OnApplicationActivated;
                Application.Current.Deactivated -= OnApplicationDeactivated;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Captures the mouse, hiding it and trapping it inside the window bounds.
        /// </summary>
        /// <remarks>
        /// This method is useful for tooling scenarios where you only care about the mouse deltas
        /// and want the user to be able to continue interacting with the window while they move
        /// the mouse. A good example of this is rotating an object based on the mouse deltas where
        /// through capturing you can spin and spin without having the cursor leave the window.
        /// </remarks>
        public new void CaptureMouse()
        {
            // Don't do anything if the mouse is already captured
            if (_isMouseCaptured || _hWnd == IntPtr.Zero)
                return;

            NativeMethods.SetCapture(_hWnd);
            _isMouseCaptured = true;
        }

        /// <summary>
        /// Releases the capture of the mouse which makes it visible and allows it to leave the window bounds.
        /// </summary>
        public new void ReleaseMouseCapture()
        {
            // Don't do anything if the mouse is not captured
            if (!_isMouseCaptured)
                return;

            NativeMethods.ReleaseCapture();
            _isMouseCaptured = false;
        }

        #endregion

        #region Graphics Device Control Implementation

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateRenderingSubscription();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UpdateRenderingSubscription();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateRenderingSubscription();
        }

        private void UpdateRenderingSubscription()
        {
            bool shouldSubscribe = !_isDisposed && IsLoaded && IsVisible && _hWnd != IntPtr.Zero;
            if (shouldSubscribe == _isRenderingSubscribed)
                return;

            if (shouldSubscribe)
            {
                CompositionTarget.Rendering += OnCompositionTargetRendering;
                _isRenderingSubscribed = true;
            }
            else
            {
                CompositionTarget.Rendering -= OnCompositionTargetRendering;
                _isRenderingSubscribed = false;
            }
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            if (_isDisposed || !IsLoaded || !IsVisible || _hWnd == IntPtr.Zero)
                return;

            // Get the current width and height of the control
            var width = (int)ActualWidth;
            var height = (int)ActualHeight;

            // If the control has no width or no height, skip drawing since it's not visible
            if (width < 1 || height < 1)
                return;

            Render(_hWnd);
        }

        protected abstract void Render(IntPtr windowHandle);

        private void OnApplicationActivated(object sender, EventArgs e)
        {
            _applicationHasFocus = true;
        }

        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
            _applicationHasFocus = false;
            ResetMouseState();

            if (_mouseInWindow)
            {
                _mouseInWindow = false;
                RaiseHwndMouseLeave(new HwndMouseEventArgs(_mouseState));
            }

            ReleaseMouseCapture();
        }

        private void ResetMouseState()
        {
            // We need to invoke events for any buttons that were pressed
            bool fireL = _mouseState.LeftButton == MouseButtonState.Pressed;
            bool fireM = _mouseState.MiddleButton == MouseButtonState.Pressed;
            bool fireR = _mouseState.RightButton == MouseButtonState.Pressed;
            bool fireX1 = _mouseState.X1Button == MouseButtonState.Pressed;
            bool fireX2 = _mouseState.X2Button == MouseButtonState.Pressed;

            // Update the state of all of the buttons
            _mouseState.LeftButton = MouseButtonState.Released;
            _mouseState.MiddleButton = MouseButtonState.Released;
            _mouseState.RightButton = MouseButtonState.Released;
            _mouseState.X1Button = MouseButtonState.Released;
            _mouseState.X2Button = MouseButtonState.Released;

            // Fire any events
            var args = new HwndMouseEventArgs(_mouseState);
            if (fireL)
                RaiseHwndLButtonUp(args);
            if (fireM)
                RaiseHwndMButtonUp(args);
            if (fireR)
                RaiseHwndRButtonUp(args);
            if (fireX1)
                RaiseHwndX1ButtonUp(args);
            if (fireX2)
                RaiseHwndX2ButtonUp(args);

            // The mouse is no longer considered to be in our window
            _mouseInWindow = false;
        }

        #endregion

        #region HWND Management

        // HwndHost lifecycle hook: WPF supplies the top-level Window's HWND (hwndParent), and we create
        // a native WS_CHILD | WS_VISIBLE sub-window owned by that parent so external renderers get a real HWND
        // while WPF's layout engine automatically repositions/resizes the child HWND during docking changes.
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            _hWnd = CreateHostWindow(hwndParent.Handle);
            UpdateRenderingSubscription();
            return new HandleRef(this, _hWnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            if (hwnd.Handle != IntPtr.Zero)
            {
                NativeMethods.DestroyWindow(hwnd.Handle);
            }

            _hWnd = IntPtr.Zero;
            UpdateRenderingSubscription();
        }

        /// <summary>
        /// Creates the host window as a child of the parent window.
        /// </summary>
        private IntPtr CreateHostWindow(IntPtr hWndParent)
        {
            RegisterWindowClass();

            // FrameworkElement.Width and Height default to double.NaN in WPF. Converting double.NaN directly
            // to int yields 0x80000000 (CW_USEDEFAULT), which is invalid for WS_CHILD windows in CreateWindowExW.
            // Clamp initial dimensions to >= 0; HwndHost updates the child HWND bounds once WPF layout completes.
            int initialWidth = GetSafeChildDimension(ActualWidth, Width);
            int initialHeight = GetSafeChildDimension(ActualHeight, Height);

            return NativeMethods.CreateWindowEx(
                0,
                WindowClass,
                string.Empty,
                NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE,
                0,
                0,
                initialWidth,
                initialHeight,
                hWndParent,
                IntPtr.Zero,
                NativeMethods.GetModuleHandle(null),
                IntPtr.Zero);
        }

        /// <summary>
        /// Registers the Unicode Win32 child window class once per process.
        /// </summary>
        private static void RegisterWindowClass()
        {
            lock (WindowClassSyncRoot)
            {
                if (_windowClassRegistered)
                {
                    return;
                }

                // NativeMethods.DefaultWindowProc is a static readonly delegate, keeping the unmanaged
                // callback thunk pinned against garbage collection for the lifetime of the 64-bit process.
                var wndClass = new NativeMethods.WNDCLASSEX
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.WNDCLASSEX)),
                    hInstance = NativeMethods.GetModuleHandle(null),
                    lpfnWndProc = NativeMethods.DefaultWindowProc,
                    lpszClassName = WindowClass,
                    hCursor = NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW)
                };

                ushort atom = NativeMethods.RegisterClassEx(ref wndClass);
                if (atom != 0 || Marshal.GetLastWin32Error() == NativeMethods.ERROR_CLASS_ALREADY_EXISTS)
                {
                    _windowClassRegistered = true;
                }
            }
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

        #endregion

        #region WndProc Implementation

        // Intercepts raw Win32 messages sent to the child HWND (subclassed by WPF's HwndHost).
        // On 64-bit Windows (x64), WPARAM and LPARAM are 8-byte IntPtr values whose upper bits of the low
        // 32-bit word (bit 31) are set whenever wheel deltas are negative (scrolling down -> 0xFF880000)
        // or client/multi-monitor mouse coordinates are negative (e.g., y = -1 -> 0xFFFFxxxx).
        // All bit extraction below routes through NativeMethods.UncheckedInt32 / IntPtr helpers so 64-bit
        // pointer values never trigger checked Int32 OverflowExceptions.
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeMethods.WM_MOUSEWHEEL:
                    if (_mouseInWindow)
                    {
                        int delta = NativeMethods.GetWheelDeltaWParam(wParam);
                        RaiseHwndMouseWheel(new HwndMouseEventArgs(_mouseState, delta, 0));
                    }
                    break;
                case NativeMethods.WM_LBUTTONDOWN:
                    _mouseState.LeftButton = MouseButtonState.Pressed;
                    RaiseHwndLButtonDown(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_LBUTTONUP:
                    _mouseState.LeftButton = MouseButtonState.Released;
                    RaiseHwndLButtonUp(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_LBUTTONDBLCLK:
                    RaiseHwndLButtonDblClick(new HwndMouseEventArgs(_mouseState, MouseButton.Left));
                    break;
                case NativeMethods.WM_RBUTTONDOWN:
                    _mouseState.RightButton = MouseButtonState.Pressed;
                    RaiseHwndRButtonDown(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_RBUTTONUP:
                    _mouseState.RightButton = MouseButtonState.Released;
                    RaiseHwndRButtonUp(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_RBUTTONDBLCLK:
                    RaiseHwndRButtonDblClick(new HwndMouseEventArgs(_mouseState, MouseButton.Right));
                    break;
                case NativeMethods.WM_MBUTTONDOWN:
                    _mouseState.MiddleButton = MouseButtonState.Pressed;
                    RaiseHwndMButtonDown(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_MBUTTONUP:
                    _mouseState.MiddleButton = MouseButtonState.Released;
                    RaiseHwndMButtonUp(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_MBUTTONDBLCLK:
                    RaiseHwndMButtonDblClick(new HwndMouseEventArgs(_mouseState, MouseButton.Middle));
                    break;
                case NativeMethods.WM_XBUTTONDOWN:
                {
                    int wParamBits = NativeMethods.UncheckedInt32(wParam);
                    if (IsXButton1(wParamBits))
                    {
                        _mouseState.X1Button = MouseButtonState.Pressed;
                        RaiseHwndX1ButtonDown(new HwndMouseEventArgs(_mouseState));
                    }
                    else if (IsXButton2(wParamBits))
                    {
                        _mouseState.X2Button = MouseButtonState.Pressed;
                        RaiseHwndX2ButtonDown(new HwndMouseEventArgs(_mouseState));
                    }
                    break;
                }
                case NativeMethods.WM_XBUTTONUP:
                {
                    int wParamBits = NativeMethods.UncheckedInt32(wParam);
                    if (IsXButton1(wParamBits))
                    {
                        _mouseState.X1Button = MouseButtonState.Released;
                        RaiseHwndX1ButtonUp(new HwndMouseEventArgs(_mouseState));
                    }
                    else if (IsXButton2(wParamBits))
                    {
                        _mouseState.X2Button = MouseButtonState.Released;
                        RaiseHwndX2ButtonUp(new HwndMouseEventArgs(_mouseState));
                    }
                    break;
                }
                case NativeMethods.WM_XBUTTONDBLCLK:
                {
                    int wParamBits = NativeMethods.UncheckedInt32(wParam);
                    if (IsXButton1(wParamBits))
                        RaiseHwndX1ButtonDblClick(new HwndMouseEventArgs(_mouseState, MouseButton.XButton1));
                    else if (IsXButton2(wParamBits))
                        RaiseHwndX2ButtonDblClick(new HwndMouseEventArgs(_mouseState, MouseButton.XButton2));
                    break;
                }
                case NativeMethods.WM_MOUSEMOVE:
                    // If the application isn't in focus, we don't handle this message
                    if (!_applicationHasFocus)
                        break;

                    // Unpack signed 16-bit client coordinates safely from the 64-bit LPARAM IntPtr.
                    _mouseState.ScreenPosition = PointToScreen(new Point(
                        NativeMethods.GetXLParam(lParam),
                        NativeMethods.GetYLParam(lParam)));

                    if (!_mouseInWindow)
                    {
                        _mouseInWindow = true;

                        RaiseHwndMouseEnter(new HwndMouseEventArgs(_mouseState));

                        // Save the previously focused HWND (full 64-bit IntPtr) and focus our child HWND
                        // so mouse-wheel messages route directly to this viewport.
                        _hWndPrev = NativeMethods.GetFocus();
                        NativeMethods.SetFocus(_hWnd);

                        // Request a one-shot WM_MOUSELEAVE notification from Win32 when the cursor exits.
                        var tme = new NativeMethods.TRACKMOUSEEVENT
                        {
                            cbSize = Marshal.SizeOf(typeof(NativeMethods.TRACKMOUSEEVENT)),
                            dwFlags = NativeMethods.TME_LEAVE,
                            hWnd = hwnd
                        };
                        NativeMethods.TrackMouseEvent(ref tme);
                    }

                    if (_mouseState.ScreenPosition != _previousPosition)
                        RaiseHwndMouseMove(new HwndMouseEventArgs(_mouseState));

                    _previousPosition = _mouseState.ScreenPosition;

                    break;
                case NativeMethods.WM_MOUSELEAVE:
                    // If we have capture, we ignore this message because we're just
                    // going to reset the cursor position back into the window
                    if (_isMouseCaptured)
                        break;

                    // Reset the state which releases all buttons and 
                    // marks the mouse as not being in the window.
                    ResetMouseState();

                    RaiseHwndMouseLeave(new HwndMouseEventArgs(_mouseState));

                    if (_hWndPrev != IntPtr.Zero)
                    {
                        NativeMethods.SetFocus(_hWndPrev);
                    }

                    break;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        private static bool IsXButton1(int wParamBits)
        {
            return NativeMethods.HighWord(wParamBits) == NativeMethods.XBUTTON1 ||
                   (wParamBits & NativeMethods.MK_XBUTTON1) != 0;
        }

        private static bool IsXButton2(int wParamBits)
        {
            return NativeMethods.HighWord(wParamBits) == NativeMethods.XBUTTON2 ||
                   (wParamBits & NativeMethods.MK_XBUTTON2) != 0;
        }

        protected virtual void RaiseHwndLButtonDown(HwndMouseEventArgs args)
        {
            var handler = HwndLButtonDown;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndLButtonUp(HwndMouseEventArgs args)
        {
            var handler = HwndLButtonUp;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndRButtonDown(HwndMouseEventArgs args)
        {
            var handler = HwndRButtonDown;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndRButtonUp(HwndMouseEventArgs args)
        {
            var handler = HwndRButtonUp;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMButtonDown(HwndMouseEventArgs args)
        {
            var handler = HwndMButtonDown;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMButtonUp(HwndMouseEventArgs args)
        {
            var handler = HwndMButtonUp;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndLButtonDblClick(HwndMouseEventArgs args)
        {
            var handler = HwndLButtonDblClick;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndRButtonDblClick(HwndMouseEventArgs args)
        {
            var handler = HwndRButtonDblClick;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMButtonDblClick(HwndMouseEventArgs args)
        {
            var handler = HwndMButtonDblClick;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMouseEnter(HwndMouseEventArgs args)
        {
            var handler = HwndMouseEnter;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX1ButtonDown(HwndMouseEventArgs args)
        {
            var handler = HwndX1ButtonDown;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX1ButtonUp(HwndMouseEventArgs args)
        {
            var handler = HwndX1ButtonUp;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX2ButtonDown(HwndMouseEventArgs args)
        {
            var handler = HwndX2ButtonDown;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX2ButtonUp(HwndMouseEventArgs args)
        {
            var handler = HwndX2ButtonUp;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX1ButtonDblClick(HwndMouseEventArgs args)
        {
            var handler = HwndX1ButtonDblClick;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX2ButtonDblClick(HwndMouseEventArgs args)
        {
            var handler = HwndX2ButtonDblClick;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMouseLeave(HwndMouseEventArgs args)
        {
            var handler = HwndMouseLeave;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMouseMove(HwndMouseEventArgs args)
        {
            var handler = HwndMouseMove;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMouseWheel(HwndMouseEventArgs args)
        {
            var handler = HwndMouseWheel;
            if (handler != null)
                handler(this, args);
        }

        #endregion
    }
}
