using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Pinpoint.Core.Converters;

namespace Pinpoint.Core.Snippets
{
    public class OcrTextSnippet : TextSnippet
    {
        public OcrTextSnippet() : base("", "")
        {
        }

        public OcrTextSnippet(string title, IEnumerable<Tuple<string, Bitmap>> transcriptions) : base(title, "")
        {
            foreach (var (content, bitmap) in transcriptions)
            {
                Transcriptions.Add(new Tuple<string, string>(content, BitmapToBase64Converter.Convert(bitmap)));
            }
        }

        public List<Tuple<string, string>> Transcriptions { get; set; } = new List<Tuple<string, string>>();

        [JsonIgnore]
        public new string RawContent
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

        public override void SaveAsJson(bool overwrite = true)
        {
            // Ensure file exists before overwriting
            base.SaveAsJson(overwrite);
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this));

        public new void SaveAsMarkdown()
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
