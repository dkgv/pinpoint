using System.Drawing;
using System.Windows;
using Pinpoint.Win.Extensions;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for SnippetWindow.xaml
    /// </summary>
    public partial class NewSnippetWindow : Window
    {
        public NewSnippetWindow(Bitmap screenshot)
        {
            InitializeComponent();

        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }
    }
}
