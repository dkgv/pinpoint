using System.Collections.ObjectModel;
using Pinpoint.Core.Snippets;

namespace Pinpoint.Win.Models
{
    internal class MainWindowModel : BasicWindowModel
    {
        public ObservableCollection<AbstractSnippet> Results { get; } = new ObservableCollection<AbstractSnippet>();
    }
}
