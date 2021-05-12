using System.Collections.ObjectModel;

namespace Pinpoint.Win.ViewModels
{
    internal class OcrSnippetWindowModel : BaseControlModel
    {
        public ObservableCollection<BitmapTextPair> BitmapPairs { get; } = new ObservableCollection<BitmapTextPair>();
    }
}
