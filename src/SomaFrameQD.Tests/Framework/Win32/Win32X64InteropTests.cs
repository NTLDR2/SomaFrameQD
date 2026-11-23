using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xaml.Behaviors;
using SomaFrameQD.Framework.Behaviors;
using SomaFrameQD.Framework.Controls;
using SomaFrameQD.Framework.Win32;

namespace SomaFrameQD.Tests.Framework.Win32
{
    [STATestClass]
    [DoNotParallelize]
    public class Win32X64InteropTests
    {
        [STATestMethod]
        public void RuntimeProcess_Is64BitOnTargetTestLane()
        {
            Assert.IsTrue(Environment.Is64BitProcess, "Expected SomaFrameQD.Tests to execute as a 64-bit (x64) process.");
            Assert.AreEqual(8, IntPtr.Size, "Expected 8-byte IntPtr on x64.");
        }

        [STATestMethod]
        public void GetWheelDeltaWParam_NegativeWheelScrollWithHighBitSetOnX64_ReturnsMinus120WithoutOverflow()
        {
            // Scrolling down (-120 = 0xFF88 in HIWORD) sets bit 31 in 64-bit WPARAM (0x00000000FF880000 = 4,287,102,976 > int.MaxValue).
            var zeroExtendedDownScroll = new IntPtr(unchecked((long)0x00000000FF880000UL));
            var signExtendedDownScroll = new IntPtr(unchecked((long)0xFFFFFFFFFF880000UL));
            var upScroll = new IntPtr(unchecked((long)0x0000000000780000UL));

            Assert.AreEqual(-120, NativeMethods.GetWheelDeltaWParam(zeroExtendedDownScroll));
            Assert.AreEqual(-120, NativeMethods.GetWheelDeltaWParam(signExtendedDownScroll));
            Assert.AreEqual(120, NativeMethods.GetWheelDeltaWParam(upScroll));
        }

        [STATestMethod]
        public void GetXLParam_GetYLParam_NegativeSignedMultiMonitorCoordinatesOnX64_UnpackSignedShortsWithoutOverflow()
        {
            // X = -320 (0xFEC0), Y = -180 (0xFF4C) -> low 32 bits = 0xFF4CFEC0 (bit 31 set).
            var zeroExtendedCoords = new IntPtr(unchecked((long)0x00000000FF4CFEC0UL));
            var signExtendedCoords = new IntPtr(unchecked((long)0xFFFFFFFFFF4CFEC0UL));

            Assert.AreEqual(-320, NativeMethods.GetXLParam(zeroExtendedCoords));
            Assert.AreEqual(-180, NativeMethods.GetYLParam(zeroExtendedCoords));
            Assert.AreEqual(-320, NativeMethods.GetXLParam(signExtendedCoords));
            Assert.AreEqual(-180, NativeMethods.GetYLParam(signExtendedCoords));
        }

        [STATestMethod]
        public void HwndWrapper_SimulatedMouseWheelAndNegativeMouseMoveMessagesOnX64_DoNotThrowOverflowException()
        {
            // Note: Do not await Background-priority dispatcher drains while HwndWrapper is alive in an
            // offscreen Window, because CompositionTarget.Rendering runs at DispatcherPriority.Render (7)
            // and can starve DispatcherPriority.Background (4) until HwndWrapper.Dispose() unhooks it.
            using (var wrapper = new TestHwndWrapper())
            {
                var window = new Window
                {
                    Width = 240,
                    Height = 180,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Left = -10000,
                    Top = -10000,
                    Content = wrapper
                };

                try
                {
                    window.Show();
                    window.UpdateLayout();

                    Assert.AreNotEqual(IntPtr.Zero, wrapper.Handle, "Expected child HWND to be created with default NaN Width/Height.");
                    wrapper.EnsureForegroundFocusForTest();

                    int mouseEnterCount = 0;
                    int mouseMoveCount = 0;
                    int? lastWheelDelta = null;
                    int x1DownCount = 0;
                    int x1UpCount = 0;

                    wrapper.HwndMouseEnter += (_, __) => mouseEnterCount++;
                    wrapper.HwndMouseMove += (_, __) => mouseMoveCount++;
                    wrapper.HwndMouseWheel += (_, e) => lastWheelDelta = e.WheelDelta;
                    wrapper.HwndX1ButtonDown += (_, __) => x1DownCount++;
                    wrapper.HwndX1ButtonUp += (_, __) => x1UpCount++;

                    // X = -25 (0xFFE7), Y = -10 (0xFFF6) -> 0x00000000FFF6FFE7 (exceeds int.MaxValue on x64).
                    var negativeMoveLParam = new IntPtr(unchecked((long)0x00000000FFF6FFE7UL));
                    NativeMethods.SendMessage(wrapper.Handle, (uint)NativeMethods.WM_MOUSEMOVE, IntPtr.Zero, negativeMoveLParam);

                    // Downward wheel scroll (-120) -> 0x00000000FF880000 (exceeds int.MaxValue on x64).
                    var downWheelWParam = new IntPtr(unchecked((long)0x00000000FF880000UL));
                    NativeMethods.SendMessage(wrapper.Handle, (uint)NativeMethods.WM_MOUSEWHEEL, downWheelWParam, IntPtr.Zero);

                    // XButton1 down (high bit set in synthetic 64-bit WPARAM) and standard Win32 XButton1 up (HIWORD = XBUTTON1).
                    var x1DownWParam = new IntPtr(unchecked((long)0x8000000000010020UL));
                    var x1UpWParam = new IntPtr(unchecked((long)0x0000000000010000UL));
                    NativeMethods.SendMessage(wrapper.Handle, (uint)NativeMethods.WM_XBUTTONDOWN, x1DownWParam, IntPtr.Zero);
                    NativeMethods.SendMessage(wrapper.Handle, (uint)NativeMethods.WM_XBUTTONUP, x1UpWParam, IntPtr.Zero);

                    Assert.AreEqual(1, mouseEnterCount);
                    Assert.AreEqual(1, mouseMoveCount);
                    Assert.AreEqual(-120, lastWheelDelta);
                    Assert.AreEqual(1, x1DownCount);
                    Assert.AreEqual(1, x1UpCount);
                }
                finally
                {
                    window.Content = null;
                    wrapper.Dispose();
                    window.Close();
                }
            }
        }

        [STATestMethod]
        public void WindowOptionsBehavior_BeforeAndAfterSourceInitialized_UpdatesStylesCleanlyOnX64()
        {
            var window = new Window
            {
                Width = 300,
                Height = 200,
                ShowInTaskbar = false,
                ShowActivated = false
            };

            try
            {
                var behavior = new WindowOptionsBehavior
                {
                    ShowMinimizeBox = false,
                    ShowMaximizeBox = false,
                    ShowIcon = false
                };

                // Attach and mutate before HWND exists (Handle == IntPtr.Zero) — must not fail or throw.
                Interaction.GetBehaviors(window).Add(behavior);
                Assert.AreEqual(IntPtr.Zero, new WindowInteropHelper(window).Handle);

                // Allocate real 64-bit Win32 HWND, triggering SourceInitialized.
                var hwnd = new WindowInteropHelper(window).EnsureHandle();
                Assert.AreNotEqual(IntPtr.Zero, hwnd);

                long styleAfterInit = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_STYLE).ToInt64();
                long exStyleAfterInit = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt64();

                Assert.AreEqual(0L, styleAfterInit & NativeMethods.WS_MINIMIZEBOX);
                Assert.AreEqual(0L, styleAfterInit & NativeMethods.WS_MAXIMIZEBOX);
                Assert.AreNotEqual(0L, exStyleAfterInit & NativeMethods.WS_EX_DLGMODALFRAME);

                // Toggle back on after SourceInitialized and verify live 64-bit SetWindowLongPtr updates.
                behavior.ShowMinimizeBox = true;
                behavior.ShowMaximizeBox = true;

                long updatedStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_STYLE).ToInt64();
                Assert.AreNotEqual(0L, updatedStyle & NativeMethods.WS_MINIMIZEBOX);
                Assert.AreNotEqual(0L, updatedStyle & NativeMethods.WS_MAXIMIZEBOX);
            }
            finally
            {
                window.Close();
            }
        }

        [STATestMethod]
        public void ClippingHwndHost_WithDefaultNaNDimensions_BuildsAndDisposesChildHwndCleanlyOnX64()
        {
            using (var clippingHost = new ClippingHwndHost { Content = new Border() })
            {
                var window = new Window
                {
                    Width = 220,
                    Height = 160,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Left = -10000,
                    Top = -10000,
                    Content = clippingHost
                };

                try
                {
                    window.Show();
                    window.UpdateLayout();

                    Assert.AreNotEqual(IntPtr.Zero, clippingHost.Handle, "Expected ClippingHwndHost HwndSource handle to be allocated.");
                    window.Width = 260;
                    window.Height = 190;
                    window.UpdateLayout();
                }
                finally
                {
                    window.Content = null;
                    clippingHost.Dispose();
                    window.Close();
                }
            }
        }

        [STATestMethod]
        public void HwndWrapper_RenderLoop_FiresOnlyWhileLoadedAndVisible()
        {
            using (var wrapper = new TestHwndWrapper())
            {
                Assert.IsFalse(wrapper.IsRenderingSubscribed, "Unattached HwndWrapper must not subscribe to CompositionTarget.Rendering.");

                var window = new Window
                {
                    Width = 240,
                    Height = 180,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Left = -10000,
                    Top = -10000,
                    Content = wrapper
                };

                try
                {
                    window.Show();
                    window.UpdateLayout();
                    Assert.IsTrue(wrapper.IsLoaded);
                    Assert.IsTrue(wrapper.IsVisible);
                    Assert.IsTrue(wrapper.IsRenderingSubscribed, "Loaded + visible HwndWrapper should subscribe to CompositionTarget.Rendering.");

                    // Collapsing (e.g., switching away from a docked tab or hiding a tool pane) must detach VSync rendering.
                    wrapper.Visibility = Visibility.Collapsed;
                    window.UpdateLayout();
                    Assert.IsFalse(wrapper.IsVisible);
                    Assert.IsFalse(wrapper.IsRenderingSubscribed, "Collapsed HwndWrapper must unsubscribe from CompositionTarget.Rendering.");

                    // Restoring visibility must re-attach VSync rendering automatically.
                    wrapper.Visibility = Visibility.Visible;
                    window.UpdateLayout();
                    Assert.IsTrue(wrapper.IsVisible);
                    Assert.IsTrue(wrapper.IsRenderingSubscribed, "Re-shown HwndWrapper must re-subscribe to CompositionTarget.Rendering.");

                    // Detaching from visual tree must unsubscribe cleanly.
                    window.Content = null;
                    window.UpdateLayout();
                    Assert.IsFalse(wrapper.IsRenderingSubscribed, "Unloaded HwndWrapper must unsubscribe from CompositionTarget.Rendering.");
                }
                finally
                {
                    window.Content = null;
                    wrapper.Dispose();
                    Assert.IsFalse(wrapper.IsRenderingSubscribed, "Disposed HwndWrapper must remain unsubscribed.");
                    window.Close();
                }
            }
        }

        private sealed class TestHwndWrapper : HwndWrapper
        {
            public void EnsureForegroundFocusForTest()
            {
                ApplicationHasFocus = true;
            }

            protected override void Render(IntPtr windowHandle)
            {
            }
        }
    }
}
