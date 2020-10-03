using System.Collections.ObjectModel;
using Pinpoint.Core.Snippets;

namespace Pinpoint.Win.Models
{
    internal class SettingsWindowModel : BasicWindowModel
    {
        public ObservableCollection<TextSnippet> ManualSnippets { get; } = new ObservableCollection<TextSnippet>();

        public ObservableCollection<FileSnippet> FileSnippets { get; } = new ObservableCollection<FileSnippet>();
    }
}
