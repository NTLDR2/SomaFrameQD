using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SomaFrameQD.Modules.Inspector.Win32
{
    internal static class NativeMethods
    {
        public const int SM_XVIRTUALSCREEN = 76;
        public const int SM_YVIRTUALSCREEN = 77;
        public const int SM_CXVIRTUALSCREEN = 78;
        public const int SM_CYVIRTUALSCREEN = 79;

        [StructLayout(LayoutKind.Sequential)]
        public struct NativePoint
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll", EntryPoint = "GetCursorPos", SetLastError = true)]
        public static extern bool GetCursorPos(out NativePoint point);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int nIndex);

        // Returns virtual-desktop bounds in physical Win32 pixels (matching GetCursorPos and GDI+ CopyFromScreen)
        // rather than WPF's 96-DPI SystemParameters.VirtualScreen* DIPs, which mismatch on 125%-200%+ displays.
        public static Rectangle GetVirtualScreenPhysicalBounds()
        {
            int left = GetSystemMetrics(SM_XVIRTUALSCREEN);
            int top = GetSystemMetrics(SM_YVIRTUALSCREEN);
            int width = Math.Max(1, GetSystemMetrics(SM_CXVIRTUALSCREEN));
            int height = Math.Max(1, GetSystemMetrics(SM_CYVIRTUALSCREEN));
            return new Rectangle(left, top, width, height);
        }
    }
}
