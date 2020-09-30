using System.Windows.Media;

namespace Pinpoint.Win.Models
{
    public class BitmapTextPair
    {
        public BitmapTextPair(ImageSource source, string content)
        {
            Source = source;
            Content = content;
        }

        public ImageSource Source { get; }

        public string Content { get; set; }
    }
}
