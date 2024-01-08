using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Pinpoint.Plugin.Windower;

public class WindowManager
{
    private static readonly Dictionary<IntPtr, Bitmap> IconCache = new();

    public static Bitmap GetIcon(IntPtr windowHandle)
    {
        if (!IconCache.ContainsKey(windowHandle))
        {
            GetWindowThreadProcessId(windowHandle, out var pid);

            var process = Process.GetProcessById(pid);
            IconCache[windowHandle] = Icon.ExtractAssociatedIcon(process.MainModule.FileName).ToBitmap();
        }

        return IconCache.TryGetValue(windowHandle, out Bitmap value) ? value : default;
    }

    public static void Focus(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }
        
        var current = GetForegroundWindow();
        if (current == windowHandle)
        {
            return;
        }

        if (IsIconic(windowHandle))
        {
            const int swRestore = 9;
            ShowWindow(windowHandle, swRestore);
        }

        if (IsZoomed(windowHandle))
        {
            const int swShowMaximized = 3;
            ShowWindow(windowHandle, swShowMaximized);
        }

        SetForegroundWindow(windowHandle);
    }

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsZoomed(IntPtr hWnd);
}