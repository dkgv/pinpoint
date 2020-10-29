using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Pinpoint.Plugin.ControlPanel
{
    public static class ControlPanelIconProvider
    {
        private const string DefaultIconRegistryPath = @"CLSID\{0}\DefaultIcon";

        public static Bitmap GetIcon(string subKeyName)
        {
            var registryPath = string.Format(DefaultIconRegistryPath, subKeyName);
            var subKey = Registry.ClassesRoot.OpenSubKey(registryPath);

            // Format is %SystemRoot%/system32/<name>.dll,-<int>
            var parts = subKey.GetValue("").ToString().Split(',');
            var iconPath = parts[0];
            var iconIndex = parts[1];

            var iconPointer = GetIconPointer(iconPath, iconIndex);
            if (iconPointer == IntPtr.Zero)
            {
                return null;
            }

            var icon = Icon.FromHandle(iconPointer);
            var bitmap = icon.ToBitmap();
            DestroyIcon(iconPointer);
            return bitmap;
        }

        private static IntPtr GetIconPointer(string filePath, string iconIndex = null)
        {
            var dataFilePtr = LoadLibraryEx(filePath, IntPtr.Zero, 0x00000002);
            if (dataFilePtr == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            const int groupIcon = 14;
            const int iconSize = 64;

            var iconIndexPtr = iconIndex != null
                                        ? new IntPtr(Math.Abs(Convert.ToInt32(iconIndex)))
                                        : IntPtr.Zero;
            var iconPtr = LoadImage(dataFilePtr, iconIndexPtr, 1, iconSize, iconSize, 0);
            if (iconPtr == IntPtr.Zero)
            {
                EnumResourceNamesWithId(dataFilePtr, groupIcon, (hModule, lpszType, lpszName, lParam) =>
                {
                    iconPtr = lpszName;
                    return false;
                }, IntPtr.Zero);
                iconPtr = LoadImage(dataFilePtr, iconPtr, 1, iconSize, iconSize, 0);
            }
            
            FreeLibrary(dataFilePtr);

            return iconPtr;
        }

        private delegate bool EnumResNameDelegate(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", EntryPoint = "EnumResourceNamesW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool EnumResourceNamesWithId(IntPtr hModule, uint lpszType, EnumResNameDelegate lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadImage(IntPtr hinst, IntPtr lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}