using System.Collections.ObjectModel;
using Pinpoint.Core.Snippets;

namespace Pinpoint.Win.Models
{
    internal class SettingsWindowModel : BasicWindowModel
    {
        public ObservableCollection<ManualSnippet> ManualSnippets { get; } = new ObservableCollection<ManualSnippet>();

        public ObservableCollection<FileSnippet> FileSnippets { get; } = new ObservableCollection<FileSnippet>();
    }
}
