using System.Collections.ObjectModel;
using Pinpoint.Plugin;

namespace Pinpoint.Win.Models
{
    internal class MainWindowModel : BaseWindowModel
    {
        public ObservableCollection<IQueryResult> Results { get; } = new ObservableCollection<IQueryResult>();
    }
}
