using System.Collections.ObjectModel;

namespace Pinpoint.Win.Models
{
    internal class OcrSnippetWindowModel : BaseWindowModel
    {
        public ObservableCollection<BitmapTextPair> BitmapPairs { get; } = new ObservableCollection<BitmapTextPair>();
    }
}
