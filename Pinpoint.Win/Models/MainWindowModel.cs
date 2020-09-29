using System.Collections.ObjectModel;
using Pinpoint.Core.Sources;

namespace Pinpoint.Win.Models
{
    internal class MainWindowModel : BasicWindowModel
    {
        public ObservableCollection<ISource> Results { get; } = new ObservableCollection<ISource>();
    }
}
