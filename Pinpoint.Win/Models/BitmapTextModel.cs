using System.Windows.Media;

namespace Pinpoint.Win.Models
{
    public class BitmapTextPair
    {
        public BitmapTextPair(ImageSource original, ImageSource modified, string content)
        {
            Original = original;
            Modified = modified;
            Content = content;
        }

        public ImageSource Original { get; }

        /// <summary>
        /// The scaled, potentially inverted bitmap.
        /// </summary>
        public ImageSource Modified { get; }

        public string Content { get; set; }
    }
}
