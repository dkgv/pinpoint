﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Pinpoint.Core;
using Pinpoint.Win.Models;

namespace Pinpoint.Win.View
{
    /// <summary>
    /// Interaction logic for PluginTabItem.xaml
    /// </summary>
    public partial class PluginTabItem : TabItem
    {
        public PluginTabItem(IPlugin plugin)
        {
            InitializeComponent();
            Model = new PluginTabItemModel{Plugin = plugin};
        }

        internal PluginTabItemModel Model
        {
            get => (PluginTabItemModel)DataContext;
            set => DataContext = value;
        }

        private void PluginSettings_OnAutoGeneratedColumns(object? sender, EventArgs e)
        {
            var grid = sender as DataGrid;
            grid.Columns[0].IsReadOnly = true;
        }
    }
}
