using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Pinpoint.Core.Snippets
{
    public class TextSnippet : ISnippet
    {
        private string _filePath;

        public TextSnippet()
        {
        }

        public TextSnippet(string title, string content)
        {
            Identifier = title;
            FilePath = title;
            RawContent = content;
            Tags = new Regex(@"#\w+").Matches(title).Select(match => match.Value).ToArray();
        }

        public string RawContent { get; set; }

        public string Identifier { get; set; }

        public string[] Tags { get; set; }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (!value.Contains(AppConstants.MainDirectory))
                {
                    var regex = new Regex("[<>:\"/\\|\\?\\*]");
                    var path = AppConstants.SnippetsDirectory + regex.Replace(value, "");
                    _filePath = path + ".json";
                    return;
                }

                _filePath = value;
            }
        }

        public void SaveAsJSON(bool overwrite = true)
        {
            // Ensure folder exists
            if (!Directory.Exists(AppConstants.SnippetsDirectory))
            {
                Directory.CreateDirectory(AppConstants.SnippetsDirectory);
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

        public void SaveAsMarkdown()
        {
            throw new System.NotImplementedException();
        }
    }
}
