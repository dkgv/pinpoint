using System.Collections.ObjectModel;
using Pinpoint.Core.Sources;

namespace Pinpoint.Win.Models
{
    internal class SettingsWindowModel : BasicWindowModel
    {
        public ObservableCollection<FileSource> FileSources { get; } = new ObservableCollection<FileSource>();
    }
}
