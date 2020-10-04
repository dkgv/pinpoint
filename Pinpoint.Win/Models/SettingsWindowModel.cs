using System.Collections.ObjectModel;
using Pinpoint.Core;
using Pinpoint.Core.Snippets;

namespace Pinpoint.Win.Models
{
    internal class SettingsWindowModel : BasicWindowModel
    {
        private HotkeyModel _hotkey = new HotkeyModel(AppSettings.GetStrOrDefault("hotkey", "ALT + SPACE"));

        public HotkeyModel Hotkey
        {
            get => _hotkey;
            set => SetProperty(ref _hotkey, value);
        }

        public ObservableCollection<TextSnippet> ManualSnippets { get; } = new ObservableCollection<TextSnippet>();

        public ObservableCollection<FileSnippet> FileSnippets { get; } = new ObservableCollection<FileSnippet>();
    }
}
