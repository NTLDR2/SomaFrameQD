using System;
using System.Runtime.InteropServices;

namespace SomaFrameQD.Framework.Win32
{
    // Provides 64-bit (x64) ABI-safe Win32 user32/kernel32 declarations and message-unpacking helpers
    // across Windows 7 SP1 through Windows 11 for SomaFrameQD's child-HWND hosting controls
    // (HwndWrapper, ClippingHwndHost) and top-level window style behaviors (WindowOptionsBehavior).
    internal static class NativeMethods
    {
        #region Constants

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        public const int WS_MAXIMIZEBOX = 0x10000;
        public const int WS_MINIMIZEBOX = 0x20000;

        public const int WS_EX_DLGMODALFRAME = 0x00000001;

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_FRAMECHANGED = 0x0020;

        public const uint WM_SETICON = 0x0080;

        // Child window style flags used by HwndWrapper and ClippingHwndHost.
        public const int WS_CHILD = 0x40000000;
        public const int WS_VISIBLE = 0x10000000;

        // Win32 error returned when RegisterClassExW is called for an already-registered window class.
        public const int ERROR_CLASS_ALREADY_EXISTS = 1410;

        // Mouse messages routed across non-WPF child HWND airspace boundaries by HwndWrapper.
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_RBUTTONDBLCLK = 0x0206;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_MBUTTONDBLCLK = 0x0209;
        public const int WM_MOUSEWHEEL = 0x020A;
        public const int WM_XBUTTONDOWN = 0x020B;
        public const int WM_XBUTTONUP = 0x020C;
        public const int WM_XBUTTONDBLCLK = 0x020D;
        public const int WM_MOUSELEAVE = 0x02A3;

        // Low-word key state masks and high-word button identifiers for WM_XBUTTON* messages.
        public const int MK_XBUTTON1 = 0x0020;
        public const int MK_XBUTTON2 = 0x0040;
        public const int XBUTTON1 = 0x0001;
        public const int XBUTTON2 = 0x0002;

        // Standard arrow cursor resource ID.
        public const int IDC_ARROW = 32512;

        // TrackMouseEvent flag requesting WM_MOUSELEAVE notifications when cursor exits the child HWND.
        public const uint TME_LEAVE = 0x00000002;

        #endregion

        #region Delegates and Structs

        public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // Kept statically rooted for the lifetime of the AppDomain so the 64-bit GC never collects
        // the unmanaged function-pointer thunk passed to RegisterClassExW.
        public static readonly WndProc DefaultWindowProc = DefWindowProc;

        [StructLayout(LayoutKind.Sequential)]
        public struct TRACKMOUSEEVENT
        {
            public int cbSize;
            public uint dwFlags;
            public IntPtr hWnd;
            public uint dwHoverTime;
        }

        // Explicit Unicode layout matches RegisterClassExW / CreateWindowExW / DefWindowProcW so
        // child HWNDs never mix ANSI and Unicode window procedures on Windows 7 SP1 - Windows 11.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativePoint
        {
            public int X;
            public int Y;
        }

        #endregion

        #region DllImports

        // On 64-bit Windows (IntPtr.Size == 8), user32.dll exports GetWindowLongPtrW / SetWindowLongPtrW.
        // On 32-bit processes (such as a 32-bit XAML designer loading AnyCPU SomaFrameQD.dll), user32.dll
        // only exports GetWindowLongW / SetWindowLongW. Dispatching on IntPtr.Size keeps both ABI-clean.
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8
                ? GetWindowLongPtr64(hWnd, nIndex)
                : new IntPtr(GetWindowLongPtr32(hWnd, nIndex));
        }

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : new IntPtr(SetWindowLongPtr32(hWnd, nIndex, UncheckedInt32(dwNewLong)));
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(
            IntPtr hwnd,
            IntPtr hwndInsertAfter,
            int x,
            int y,
            int width,
            int height,
            uint flags);

        [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(
            IntPtr hwnd,
            uint msg,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateWindowEx(
            int exStyle,
            string className,
            string windowName,
            int style,
            int x,
            int y,
            int width,
            int height,
            IntPtr hwndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcW", CharSet = CharSet.Unicode)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string module);

        [DllImport("user32.dll", EntryPoint = "LoadCursorW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

        [DllImport("user32.dll", EntryPoint = "RegisterClassExW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U2)]
        public static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ScreenToClient(IntPtr hWnd, ref NativePoint pt);

        // Returns the previously focused HWND as a full 64-bit pointer-sized IntPtr rather than a truncated 32-bit int.
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(ref NativePoint point);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

        #endregion

        #region Helpers

        // Extracts the low 32 bits of a 64-bit WPARAM/LPARAM IntPtr without triggering OverflowException
        // when bit 31 is set (e.g., negative mouse-wheel deltas or negative signed multi-monitor coordinates).
        public static int UncheckedInt32(IntPtr value)
        {
            return unchecked((int)value.ToInt64());
        }

        public static int GetXLParam(IntPtr lParam)
        {
            return LowWord(UncheckedInt32(lParam));
        }

        public static int GetYLParam(IntPtr lParam)
        {
            return HighWord(UncheckedInt32(lParam));
        }

        public static int GetWheelDeltaWParam(IntPtr wParam)
        {
            return HighWord(UncheckedInt32(wParam));
        }

        public static int GetXLParam(int lParam)
        {
            return LowWord(lParam);
        }

        public static int GetYLParam(int lParam)
        {
            return HighWord(lParam);
        }

        public static int GetWheelDeltaWParam(int wParam)
        {
            return HighWord(wParam);
        }

        public static int LowWord(int input)
        {
            return (short)(input & 0xffff);
        }

        public static int HighWord(int input)
        {
            return (short)(input >> 16);
        }

        #endregion
    }
}
