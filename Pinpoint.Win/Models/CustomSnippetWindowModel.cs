using System.Collections.ObjectModel;

namespace Pinpoint.Win.Models
{
    internal class CustomSnippetWindowModel : BasicWindowModel
    {
        public ObservableCollection<BitmapTextPair> BitmapPairs { get; } = new ObservableCollection<BitmapTextPair>();
    }
}
