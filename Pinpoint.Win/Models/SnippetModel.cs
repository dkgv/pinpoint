using System.Collections.Generic;
using System.Text;

namespace Pinpoint.Win.Models
{
    public class SnippetModel
    {
        public SnippetModel(string title, IEnumerable<BitmapTextPair> transcriptions)
        {
            Title = title;
            Transcriptions.AddRange(transcriptions);
        }

        public SnippetModel(string title, string content) : this(title, new[]{new BitmapTextPair(null, content) })
        {
        }

        public string Title { get; set; }

        public List<BitmapTextPair> Transcriptions { get; } = new List<BitmapTextPair>();

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
                if (transcription.Source == null)
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
