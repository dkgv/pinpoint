using System;
using System.IO;

namespace Pinpoint.Core
{
    public static class Constants
    {
        private static readonly string UserHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static readonly string MainDirectory = UserHome + Path.DirectorySeparatorChar + "Pinpoint" + Path.DirectorySeparatorChar;
        public static readonly string SettingsFilePath = MainDirectory + "settings.json";
    }
}
