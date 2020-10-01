using System.Collections.ObjectModel;
using Pinpoint.Core.Snippets;

namespace Pinpoint.Win.Models
{
    internal class MainWindowModel : BasicWindowModel
    {
        public ObservableCollection<ISnippet> Results { get; } = new ObservableCollection<ISnippet>();
    }
}
