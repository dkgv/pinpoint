using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Pinpoint.Core;
using Pinpoint.Core.Snippets;
using Pinpoint.Win.Converters;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for NewSimpleSnippetWindow.xaml
    /// </summary>
    public partial class TextSnippetWindow : Window
    {
        private readonly QueryEngine _queryEngine;

        public TextSnippetWindow(QueryEngine queryEngine)
        {
            _queryEngine = queryEngine;
            InitializeComponent();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var title = TxtTitle.Text;

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please specify a title for your snippet before saving it.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var snippet = new ManualSnippet(title, TxtContent.Text);
            if (_queryEngine.AddSnippet(this, snippet))
            {
                snippet.SaveAsJSON();
            }

            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtTitle.Focus();
        }
    }
}
