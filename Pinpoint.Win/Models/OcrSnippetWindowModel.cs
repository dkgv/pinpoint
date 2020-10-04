using System.Collections.ObjectModel;

namespace Pinpoint.Win.Models
{
    internal class OcrSnippetWindowModel : BasicWindowModel
    {
        public ObservableCollection<BitmapTextPair> BitmapPairs { get; } = new ObservableCollection<BitmapTextPair>();
    }
}
