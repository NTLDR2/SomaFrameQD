using System;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Xaml.Behaviors;
using SomaFrameQD.Framework.Win32;

namespace SomaFrameQD.Framework.Behaviors
{
    // Why WindowOptionsBehavior wraps Win32 HWND window-style flags:
    // WPF's built-in Window.ResizeMode property only offers coarse presets (NoResize, CanMinimize, CanResize)
    // and provides no managed API to hide the title-bar icon (WS_EX_DLGMODALFRAME + WM_SETICON) or toggle
    // WS_MINIMIZEBOX / WS_MAXIMIZEBOX independently on resizable modal dialogs such as SettingsView.
    // Once the underlying Win32 HWND is created (SourceInitialized), this behavior updates GWL_STYLE and
    // GWL_EXSTYLE via 64-bit-safe GetWindowLongPtr / SetWindowLongPtr calls.
    public class WindowOptionsBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
            "ShowIcon", typeof(bool), typeof(WindowOptionsBehavior),
            new PropertyMetadata(true, OnWindowOptionChanged));

        public bool ShowIcon
        {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        public static readonly DependencyProperty ShowMinimizeBoxProperty = DependencyProperty.Register(
            "ShowMinimizeBox", typeof(bool), typeof(WindowOptionsBehavior),
            new PropertyMetadata(true, OnWindowOptionChanged));

        public bool ShowMinimizeBox
        {
            get { return (bool)GetValue(ShowMinimizeBoxProperty); }
            set { SetValue(ShowMinimizeBoxProperty, value); }
        }

        public static readonly DependencyProperty ShowMaximizeBoxProperty = DependencyProperty.Register(
            "ShowMaximizeBox", typeof(bool), typeof(WindowOptionsBehavior),
            new PropertyMetadata(true, OnWindowOptionChanged));

        public bool ShowMaximizeBox
        {
            get { return (bool)GetValue(ShowMaximizeBoxProperty); }
            set { SetValue(ShowMaximizeBoxProperty, value); }
        }

        private static void OnWindowOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WindowOptionsBehavior)d).UpdateWindowStyle();
        }

        private void UpdateWindowStyle()
        {
            if (AssociatedObject == null)
                return;

            var handle = new WindowInteropHelper(AssociatedObject).Handle;

            // When ShowIcon, ShowMinimizeBox, or ShowMaximizeBox is set in XAML before Window.SourceInitialized,
            // the Win32 HWND has not been allocated yet (handle == IntPtr.Zero). Defer until OnSourceInitialized.
            if (handle == IntPtr.Zero)
                return;

            long windowStyle = NativeMethods.GetWindowLongPtr(handle, NativeMethods.GWL_STYLE).ToInt64();

            if (ShowMinimizeBox)
                windowStyle |= (long)NativeMethods.WS_MINIMIZEBOX;
            else
                windowStyle &= ~(long)NativeMethods.WS_MINIMIZEBOX;

            if (ShowMaximizeBox)
                windowStyle |= (long)NativeMethods.WS_MAXIMIZEBOX;
            else
                windowStyle &= ~(long)NativeMethods.WS_MAXIMIZEBOX;

            NativeMethods.SetWindowLongPtr(handle, NativeMethods.GWL_STYLE, new IntPtr(windowStyle));

            if (!ShowIcon)
            {
                long exWindowStyle = NativeMethods.GetWindowLongPtr(handle, NativeMethods.GWL_EXSTYLE).ToInt64();
                NativeMethods.SetWindowLongPtr(
                    handle,
                    NativeMethods.GWL_EXSTYLE,
                    new IntPtr(exWindowStyle | (long)NativeMethods.WS_EX_DLGMODALFRAME));

                NativeMethods.SetWindowPos(
                    handle,
                    IntPtr.Zero,
                    0,
                    0,
                    0,
                    0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_FRAMECHANGED);

                NativeMethods.SendMessage(handle, NativeMethods.WM_SETICON, IntPtr.Zero, IntPtr.Zero);
            }
        }

        protected override void OnAttached()
        {
            AssociatedObject.SourceInitialized += OnSourceInitialized;
            base.OnAttached();
            UpdateWindowStyle();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SourceInitialized -= OnSourceInitialized;
            base.OnDetaching();
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            UpdateWindowStyle();
        }
    }
}
