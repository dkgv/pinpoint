using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Pinpoint.Core
{
    public static class NativeProvider
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        private static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        private static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

        private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 4 ? new IntPtr(GetClassLong32(hWnd, nIndex)) : GetClassLong64(hWnd, nIndex);
        }

        public static Bitmap GetActiveWindowIcon()
        {
            // See https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-geticon
            const uint wmGeticon = 0x007f;

            var iconSmall2 = new IntPtr(2);
            var idiApplication = new IntPtr(0x7F00);
            var gclHicon = -14;

            var hWnd = GetForegroundWindow();
            var hIcon = SendMessage(hWnd, wmGeticon, iconSmall2, IntPtr.Zero);

            if (hIcon == IntPtr.Zero)
            {
                hIcon = GetClassLongPtr(hWnd, gclHicon);
            }

            if (hIcon == IntPtr.Zero)
            {
                // See https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-loadicona
                hIcon = LoadIcon(IntPtr.Zero, idiApplication);
            }

            return hIcon == IntPtr.Zero ? null : Icon.FromHandle(hIcon).ToBitmap();
        }
    }
}