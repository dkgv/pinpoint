﻿using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Win.Models
{
    internal class MainWindowModel : BaseControlModel
    {
        private ThemeModel _theme = AppSettings.GetAsOrDefault("theme", ThemeModel.DarkTheme);

        public ObservableUniqueCollection<AbstractQueryResult> Results { get; } = new ObservableUniqueCollection<AbstractQueryResult>();

        public ThemeModel Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }
    }
}
