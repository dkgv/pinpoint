using System;
using System.IO;

namespace Pinpoint.Core
{
    public static class AppConstants
    {
        private static readonly string UserHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static readonly string MainDirectory = UserHome + Path.DirectorySeparatorChar + "Pinpoint" + Path.DirectorySeparatorChar;
        public static readonly string SettingsFilePath = MainDirectory + "settings.json";
        
        public static readonly string HotkeyToggleVisibilityId = "ToggleVisibility";
        public static readonly string HotkeyPasteId = "PasteFromClipboard";
        public static readonly string HotkeyCopyId = "CopyToClipboard";

        public const string Version = "0.1.6";
    }
}
