using System.Collections.ObjectModel;

namespace Pinpoint.Win.Models
{
    internal class OcrSnippetWindowModel : BaseControlModel
    {
        public ObservableCollection<BitmapTextPair> BitmapPairs { get; } = new ObservableCollection<BitmapTextPair>();
    }
}
