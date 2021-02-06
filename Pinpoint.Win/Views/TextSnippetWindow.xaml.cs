using System.Windows;
using Pinpoint.Core;
using Pinpoint.Plugin.Snippets;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for NewSimpleSnippetWindow.xaml
    /// </summary>
    public partial class TextSnippetWindow : Window
    {
        private readonly PluginEngine _pluginEngine;

        public TextSnippetWindow(PluginEngine pluginEngine, TextSnippet snippet = null)
        {
            _pluginEngine = pluginEngine;
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
                const string msg = "Please specify a title for your snippet before saving it.";
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var snippet = new TextSnippet(title, TxtContent.Text);
            if (_pluginEngine.Plugin<SnippetsPlugin>().AddSnippet(this, snippet))
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
