using System;
using System.IO;

namespace Pinpoint.Plugin
{
    public static class AppConstants
    {
        private static readonly string UserHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static readonly string MainDirectory = UserHome + Path.DirectorySeparatorChar + "Pinpoint" + Path.DirectorySeparatorChar;
        public static readonly string SettingsFilePath = MainDirectory + "settings.json";
        public static readonly string HotkeyIdentifier = "Show/Hide";

        public const string Version = "0.0.5";
    }
}
