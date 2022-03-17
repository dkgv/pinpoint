﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using WK.Libraries.SharpClipboardNS;

namespace Pinpoint.Win.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public ObservableUniqueCollection<AbstractQueryResult> Results { get; } = new();

        public ObservableUniqueCollection<AbstractQueryResult> CacheResults { get; set; } = new();

        public PluginEngine PluginEngine { get; set; } = new();

        public SharpClipboard Clipboard { get; } = new();

        public string PreviousQuery { get; set; } = string.Empty;
    }
}
