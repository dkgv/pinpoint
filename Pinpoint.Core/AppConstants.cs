using System;
using System.IO;

namespace Pinpoint.Core
{
    public static class AppConstants
    {
        private static readonly string UserHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static readonly string MainDirectory = UserHome + Path.DirectorySeparatorChar + "Pinpoint" + Path.DirectorySeparatorChar;
        public static readonly string SettingsFilePath = MainDirectory + "settings.json";
        
        public const string HotkeyToggleVisibilityId = "ToggleVisibility";
        public const string HotkeyPasteId = "PasteFromClipboard";

        public const string Version = "0.2.6";
    }
}
