using System.Collections.ObjectModel;
using Pinpoint.Plugin;

namespace Pinpoint.Win.Models
{
    internal class MainWindowModel : BaseWindowModel
    {
        private ThemeModel _theme = AppSettings.GetAsOrDefault("theme", ThemeModel.DarkTheme);

        public ObservableCollection<AbstractQueryResult> Results { get; } = new ObservableCollection<AbstractQueryResult>();

        public ThemeModel Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }
    }
}
