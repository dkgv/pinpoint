using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.Snippets
{
    public class TextSnippet : AbstractSnippet
    {
        private string _filePath;

        public static readonly string SnippetsDirectory = AppConstants.MainDirectory + "Snippets" + Path.DirectorySeparatorChar;

        public TextSnippet()
        {
        }

        public TextSnippet(string title, string content) : base(title, content)
        {
        }

        public override string FilePath
        {
            get => _filePath;
            set
            {
                if (value != null && !value.Contains(AppConstants.MainDirectory))
                {
                    var regex = new Regex("[<>:\"/\\|\\?\\*]");
                    var path = SnippetsDirectory + regex.Replace(value, "");
                    _filePath = path + ".json";
                    return;
                }

                _filePath = value;
            }
        }

        public override void SaveAsJson(bool overwrite = true)
        {
            // Ensure folder exists
            if (!Directory.Exists(SnippetsDirectory))
            {
                Directory.CreateDirectory(SnippetsDirectory);
            }

            // Create file if non-existent
            if (File.Exists(FilePath))
            {
                if (!overwrite)
                {
                    return;
                }
            }
            else
            {
                File.Create(FilePath).Close();
            }

            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this));
            GC.Collect();
        }
    }
}
