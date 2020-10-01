using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Pinpoint.Core.Converters;

namespace Pinpoint.Core.Snippets
{
    public class ManualSnippet : ISnippet
    {
        private string _filePath;

        public ManualSnippet()
        {
        }

        public ManualSnippet(string title, IEnumerable<Tuple<string, Bitmap>> transcriptions)
        {
            Identifier = title;

            foreach (var (content, bitmap) in transcriptions)
            {
                Transcriptions.Add(new Tuple<string, string>(content, BitmapBase64Converter.ToBase64(bitmap)));
            }

            FilePath = title;
        }

        public List<Tuple<string, string>> Transcriptions { get; set; } = new List<Tuple<string, string>>();

        public string RawContent
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var (content, _) in Transcriptions)
                {
                    sb.Append(content).Append('\n');
                }
                return sb.ToString();
            }
            set { }
        }

        public string Identifier { get; set; }

        public string FilePath
        {
            get => _filePath;
            set
            {
                var regex = new Regex("[<>:\"/\\|\\?\\*]");
                _filePath = AppConstants.SnippetsDirectory + regex.Replace(value, "") + ".json";
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

            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
            GC.Collect();
        }

        public void SaveAsMarkdown()
        {
            var sb = new StringBuilder();

            sb.Append("#").Append(Identifier).Append('\n');

            foreach (var (content, base64) in Transcriptions)
            {
                sb.Append(content)
                    .Append('\n')
                    .Append("<center>")
                    .Append("![](data:image/*:base64,")
                    .Append(base64)
                    .Append("</center>")
                    .Append('\n')
                    .Append("<hr />");
            }

            // TODO write
        }
    }
}
