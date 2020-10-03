using System;
using System.IO;

namespace Pinpoint.Core
{
    public static class AppConstants
    {
        private static readonly string UserHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static readonly string MainDirectory = UserHome + Path.DirectorySeparatorChar + "Pinpoint" + Path.DirectorySeparatorChar;
        public static readonly string SnippetsDirectory = MainDirectory + "Snippets" + Path.DirectorySeparatorChar;
        public static readonly string SettingsFilePath = MainDirectory + "settings.json";

        public static readonly string FileSnippetsKey = "file_snippets";
        public static readonly string TextSnippetsKey = "text_snippets";
        public static readonly string OcrSnippetsKey = "ocr_snippets";
    }
}
