using System;
using System.Windows;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for NewSimpleSnippetWindow.xaml
    /// </summary>
    public partial class TextSnippetWindow : Window
    {
        public TextSnippetWindow()
        {
            InitializeComponent();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtTitle.Focus();
        }
    }
}
