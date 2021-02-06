using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Pinpoint.Plugin.Snippets;
using Xceed.Wpf.AvalonDock.Controls;

namespace Pinpoint.Win.View
{
    /// <summary>
    /// Interaction logic for SnippetSyntaxControl.xaml
    /// </summary>
    public partial class SnippetSyntaxControl : UserControl
    {
        public SnippetSyntaxControl()
        {
            InitializeComponent();

            foreach (var syntaxType in SyntaxTypeHelper.Values())
            {
                CbSyntax.Items.Add(SyntaxTypeHelper.ToString(syntaxType));
            }

            CbSyntax.SelectedIndex = 0;
        }

        private void CbSyntax_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var editor in Window.GetWindow(this).FindVisualChildren<TextEditor>())
            {
                editor.SyntaxHighlighting =
                    HighlightingManager.Instance.GetDefinition(CbSyntax.SelectedItem.ToString());
            }
        }
    }
}
