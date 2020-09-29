using System.Collections.Generic;
using System.Text;

namespace Pinpoint.Core.Models
{
    public class SnippetModel
    {
        public SnippetModel(string title, IEnumerable<BitmapTranscription> transcriptions)
        {
            Title = title;
            Transcriptions.AddRange(transcriptions);
        }

        public SnippetModel(string title, string content) : this(title, new[]{new BitmapTranscription(null, content) })
        {
        }

        public string Title { get; set; }

        public List<BitmapTranscription> Transcriptions { get; } = new List<BitmapTranscription>();

        public string TLDR { get; set; }

        public string ToMarkdown()
        {
            var sb = new StringBuilder();

            sb.Append("#").Append(Title).Append('\n');

            if (!string.IsNullOrEmpty(TLDR))
            {
                sb.Append("TLDR: ").Append(TLDR).Append('\n');
            }

            foreach (var transcription in Transcriptions)
            {
                if (transcription.Bitmap == null)
                {
                    sb.Append(transcription.Content).Append('\n');
                }
                else
                {

                }
            }

            return sb.ToString();
        }
    }
}
