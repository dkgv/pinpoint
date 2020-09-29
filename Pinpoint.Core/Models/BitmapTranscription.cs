using System.Drawing;

namespace Pinpoint.Core.Models
{
    public class BitmapTranscription
    {
        public BitmapTranscription(Bitmap bitmap, string content)
        {
            Bitmap = bitmap;
            Content = content;
        }

        public Bitmap Bitmap { get; }

        public string Content { get; }
    }
}
