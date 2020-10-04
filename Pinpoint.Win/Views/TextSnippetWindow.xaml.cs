using System.Windows;
using Pinpoint.Core;
using Pinpoint.Core.Snippets;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for NewSimpleSnippetWindow.xaml
    /// </summary>
    public partial class TextSnippetWindow : Window
    {
        private readonly QueryEngine _queryEngine;

        public TextSnippetWindow(QueryEngine queryEngine, TextSnippet snippet = null)
        {
            _queryEngine = queryEngine;
            InitializeComponent();

            if (snippet != null)
            {
                TxtTitle.Text = snippet.Identifier;
                TxtContent.Text = snippet.RawContent;
            }
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

            var snippet = new TextSnippet(title, TxtContent.Text);
            if (_queryEngine.AddSnippet(this, snippet))
            {
                snippet.SaveAsJson();
            }

            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtTitle.Focus();
        }
    }
}
